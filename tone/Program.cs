using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading;
using ATL;
using GrokNet;
using Jint;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Sandreas.AudioMetadata;
using Sandreas.Files;
using Sandreas.SpectreConsoleHelpers.DependencyInjection;
using Spectre.Console;
using tone;
using tone.Api.JavaScript;
using tone.Commands;
using tone.Commands.Settings;
using tone.Commands.Settings.Interfaces;
using tone.Matchers;
using tone.Metadata;
using tone.Metadata.Formats;
using tone.Metadata.Taggers;
using tone.Serializers;
using tone.Services;
using Serilog;
using Spectre.Console.Cli;
using tone.Metadata.Taggers.IdTaggers.Audible;
using ILogger = Serilog.ILogger;
using Log = Serilog.Log;


try
{
    var debugMode = args.Contains("--debug");
    
    // atldotnet basic settings to change the common behaviour
    Settings.UseFileNameWhenNoTitle = false; // don't fallback to filename when no title is set
    // Settings.NullAbsentValues = true; // use null instead of default when tags are considered empty
    Settings.OutputStacktracesToConsole = false; // don't output something to console
    // Settings.ID3v2_tagSubVersion = 3; // use id3v2.4 by default (4 for 2.4)
    // Settings.MP3_parseExactDuration = true; // more exact duration, takes longer
    
    var settingsProvider = new CustomCommandSettingsProvider();
    var services = new ServiceCollection();
    services.AddSingleton<ILogger>(_ =>
    {
        var loggerSettings = settingsProvider.Get<ILoggerSettings>();
        var configBuilder = new ConfigurationBuilder().AddEnvironmentVariables();
        var config = new LoggerConfiguration()
            .ReadFrom.Configuration(configBuilder.Build()); 
        
        // set loglevel to debug, if --debug is present
        if (debugMode && loggerSettings != null)
        {
            loggerSettings.LogLevel = LogLevel.Debug;
        } 
        
        if (loggerSettings == null || loggerSettings.LogLevel == LogLevel.None)
        {
            return config.CreateLogger();
        }

        config = config.WriteTo.File(loggerSettings.LogFile);
        config = loggerSettings.LogLevel switch
        {
            LogLevel.Trace => config.MinimumLevel.Debug(),
            LogLevel.Debug => config.MinimumLevel.Debug(),
            LogLevel.Information => config.MinimumLevel.Information(),
            LogLevel.Warning => config.MinimumLevel.Warning(),
            _ => config.MinimumLevel.Error()
        };
        
        return config.CreateLogger();
    });
    services.AddLogging(loggingBuilder => loggingBuilder.SetMinimumLevel(
        settingsProvider.Get<ILoggerSettings>()?.LogLevel ?? LogLevel.None).AddSerilog(
            )
    );
    services.AddSingleton(_ => settingsProvider);
    services.AddSingleton<StartupErrorService>();
    services.AddSingleton<HttpClient>();
    services.AddSingleton<FileSystem>();
    services.AddSingleton<FileWalker>();
    services.AddSingleton<DirectoryLoaderService>();
    services.AddSingleton<GrokPatternService>();
    services.AddSingleton<ChptFmtNativeMetadataFormat>();
    services.AddSingleton<FfmetadataFormat>();
    services.AddSingleton<ToneJsonMeta>();
    services.AddSingleton(_ => new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        ContractResolver = new ToneJsonContractResolver(settingsProvider.Get<DumpCommandSettings>()?.IncludeProperties),
        Converters = new List<JsonConverter>
        {
            new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() }
        },
        Formatting = Formatting.Indented,
    });
    services.AddSingleton<ChptFmtNativeSerializer>();
    services.AddSingleton<FfmetadataSerializer>();
    services.AddSingleton<SpectreConsoleSerializer>();
    services.AddSingleton<ToneJsonSerializer>();
    services.AddSingleton<SerializerService>();
    services.AddSingleton<SpectreConsoleService>();
    services.AddSingleton<ScriptConsole>();
    services.AddSingleton(s =>
    {
        var patternService = s.GetRequiredService<GrokPatternService>();
        var startupErrorsService = s.GetRequiredService<StartupErrorService>();
        var settings = settingsProvider.Get<IPathPatternSettings>();
        var pathPatterns = new List<(string, Grok)>();
        if (settings != null)
        {
            var customPatterns = settings.PathPatternExtension.Concat(new[]
            {
                "NOTDIRSEP [^/\\\\]*",
                "PARTNUMBER \\b[0-9-.,IVXLCDM]+\\b"
            });

            var grokDefinitions = patternService.Build(settings.PathPattern, customPatterns);
            if (grokDefinitions)
            {
                pathPatterns = grokDefinitions.Value.ToList();
            }
            else
            {
                startupErrorsService.Errors.Add((ReturnCode.GeneralError,
                    "Could not parse `--path-pattern`: " + grokDefinitions.Error));
            }
        }

        return new PathPatternMatcher(pathPatterns);
    });
    services.AddSingleton<CancellationTokenSource>();
    services.AddSingleton(sp =>
    {
        return new Engine(options =>
        {
            var cts = sp.GetRequiredService<CancellationTokenSource>();
            options.LimitMemory(4_000_000);
            options.TimeoutInterval(TimeSpan.FromSeconds(25));
            options.MaxStatements(1000);
            options.CancellationToken(cts.Token);
        });
    });

    services.AddSingleton<AudibleIdTaggerSettings>(_ => new AudibleIdTaggerSettings(){
        MetadataUrlTemplate = Environment.GetEnvironmentVariable("PILABOR_AUDIBLE_METADATA_URL_TEMPLATE") ?? "",
        ChaptersUrlTemplate = Environment.GetEnvironmentVariable("PILABOR_AUDIBLE_CHAPTERS_URL_TEMPLATE") ?? "",
        
    });
    services.AddHttpClient<AudibleIdTagger>();
    
    services.AddSingleton<IdTaggerComposite>();
    
    services.AddSingleton(sp =>
    {
        var fs = sp.GetRequiredService<FileSystem>();
        var pathMatcher = sp.GetRequiredService<PathPatternMatcher>();
        var chapterFormat = sp.GetRequiredService<ChptFmtNativeMetadataFormat>();
        var ffmetadataFormat = sp.GetRequiredService<FfmetadataFormat>();
        var idTagger  = sp.GetRequiredService<IdTaggerComposite>();
        
        var taggers = new[]
        {
            settingsProvider.Build<IToneJsonTaggerSettings, INamedTagger>(s => new ToneJsonTagger(fs, s)),
            settingsProvider.Build<IMetadata, INamedTagger>(s => new MetadataTagger(s)),
            
            settingsProvider.Build<IIdTaggerSettings, INamedTagger>(s =>
            {
                idTagger.Id = s.Id;
                return idTagger;
            }),

            settingsProvider.Build<ICoverTaggerSettings, INamedTagger>(s => new CoverTagger(fs, s)),
            settingsProvider.Build<IPathPatternSettings, INamedTagger>(_ => new PathPatternTagger(pathMatcher)),
            settingsProvider.Build<IFfmetadataTaggerSettings, INamedTagger>(s =>
                new FfmetadataTagger(fs, ffmetadataFormat, s.ImportFfmetadataFile, s.AutoImportFfmetadata)),
            settingsProvider.Build<IChptFmtNativeTaggerSettings, INamedTagger>(s =>
                new ChptFmtNativeTagger(fs, chapterFormat, s.ImportChaptersFile, s.AutoImportChapters)),
            settingsProvider.Build<IEquateTaggerSettings, INamedTagger>(s => new EquateTagger(s)),
            new M4BFillUpTagger(),
            settingsProvider.Build<IPrependMovementToDescriptionTaggerSettings, INamedTagger>(s =>
                new PrependMovementToDescriptionTagger(s.PrependMovementToDescription)),
            settingsProvider.Build<IRemoveTaggerSettings, INamedTagger>(s => new RemoveTagger(s))
        }.Where(t => t != null).Select(e => e!).ToArray();
        var taggerOrderSettings = settingsProvider.Get<ITaggerOrderSettings>();
        return taggerOrderSettings == null
            ? new TaggerComposite(taggers)
            : new TaggerComposite(taggerOrderSettings.Taggers, taggers);
    });
    services.AddSingleton<JavaScriptApi>(sp =>
    {
        var http = sp.GetRequiredService<HttpClient>();
        var fs = sp.GetRequiredService<FileSystem>();
        var jint = sp.GetRequiredService<Engine>();
        var scriptConsole = sp.GetRequiredService<ScriptConsole>();
        var taggerComposite = sp.GetRequiredService<TaggerComposite>();
        var idTagger = sp.GetRequiredService<IdTaggerComposite>();
        var script = "";
        var javaScriptApi = settingsProvider.Build<IScriptSettings, JavaScriptApi>(s =>
        {
            script = s.Scripts.Aggregate(script, (current, scr) => current + fs.File.ReadAllText(scr));
            return new JavaScriptApi(jint, fs, http, taggerComposite, idTagger, s);
        }) ?? new JavaScriptApi();

        jint.SetValue("tone", javaScriptApi);
        jint.SetValue("console", scriptConsole);
        jint.Execute(script);
        return javaScriptApi;
    });

// services.AddSingleton(_ => AnsiConsole.Console);
// services.AddLogging(builder => builder.AddSerilog(dispose: true));
// services.AddSingleton<ILogger>(_ => Log.Logger);


    var app = new CommandApp(new CustomTypeRegistrar(services));

    app.Configure(config =>
    {
        config.SetInterceptor(new CustomCommandInterceptor(settingsProvider));
        config.UseStrictParsing();
        config.CaseSensitivity(CaseSensitivity.None);
        config.SetApplicationName("tone");
        config.SetApplicationVersion("0.2.0");
        config.ValidateExamples();
        config.AddCommand<DumpCommand>("dump")
            .WithDescription("dump metadata for files and directories (directories are traversed recursively)")
            .WithExample("dump", "--help")
            .WithExample("dump", "input.mp3")
            .WithExample("dump", "audio-directory/", "--include-extension", "m4b", "--include-extension", "mp3", "--format", "ffmetadata", "--include-property", "title", "--include-property", "artist")
            .WithExample("dump", "input.mp3", "--format", "json", "--query", "$.meta.album")
            ;
        config.AddCommand<TagCommand>("tag")
            .WithDescription("tag files with metadata properties (directories are traversed recursively)")
            .WithExample("tag", "--help")
            .WithExample("tag", "input.mp3", "--meta-title", "\"a title\"")
            .WithExample("tag", "--debug", "--auto-import=covers", "--meta-additional-field", "©st3=testing", "input.m4b", "--dry-run")
            .WithExample("tag", "--auto-import=covers", "--auto-import=chapters", "--path-pattern=\"audiobooks/%g/%a/%s/%p - %n.m4b\"", "--path-pattern=\"audiobooks/%g/%a/%z/%n.m4b\"", "audiobooks/", "--dry-run")
            .WithExample("tag", "input.mp3", "--script", "musicbrainz.js", "--script-tagger-parameter", "e2310769-2e68-462f-b54f-25ac8e3f1a21")
            ;

        /*config.AddCommand<SplitCommand>("split")
            .WithDescription("split audio files")
            ;    
            */

        if (debugMode)
        {
            config.PropagateExceptions();
        }
#if DEBUG
        config.ValidateExamples();
#endif
    });

    return await app.RunAsync(args).ConfigureAwait(false);
}
catch (Exception e)
{
    if (e is CommandParseException { Pretty: { } } ce)
    {
        AnsiConsole.Write(ce.Pretty);
    }

    AnsiConsole.WriteException(e);
    return (int)ReturnCode.UncaughtException;
}
finally
{
    Log.CloseAndFlush();
}