using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading;
using GrokNet;
using Jint;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Sandreas.AudioMetadata;
using Sandreas.Files;
using Spectre.Console;
using Spectre.Console.Cli;
using tone;
using tone.Api.JavaScript;
using tone.Commands;
using tone.Commands.Settings;
using tone.Commands.Settings.Interfaces;
using tone.DependencyInjection;
using tone.Interceptors;
using tone.Matchers;
using tone.Metadata;
using tone.Metadata.Formats;
using tone.Metadata.Taggers;
using tone.Serializers;
using tone.Services;

var propagateExceptions = args.Contains("--debug");

var settingsProvider = new CommandSettingsProvider();

var services = new ServiceCollection();

services.AddSingleton(_ => settingsProvider);
services.AddSingleton<StartupErrorService>();
services.AddSingleton<HttpClient>();
// services.AddTransient<StringWriter>();
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


services.AddSingleton(sp =>
{
    var fs = sp.GetRequiredService<FileSystem>();
    var pathMatcher = sp.GetRequiredService<PathPatternMatcher>();
    var chapterFormat = sp.GetRequiredService<ChptFmtNativeMetadataFormat>();
    var taggers = new[]
    {
        settingsProvider.Build<IToneJsonTaggerSettings, INamedTagger>(s => new ToneJsonTagger(fs, s)),
        settingsProvider.Build<IMetadata, INamedTagger>(s => new MetadataTagger(s)),
        settingsProvider.Build<ICoverTaggerSettings, INamedTagger>(s => new CoverTagger(fs, s)),
        settingsProvider.Build<IPathPatternSettings, INamedTagger>(_ => new PathPatternTagger(pathMatcher)),
        settingsProvider.Build<IChptFmtNativeTaggerSettings, INamedTagger>(s =>
            new ChptFmtNativeTagger(fs, chapterFormat, s.ImportChaptersFile, s.AutoImportChapters)),
        settingsProvider.Build<IEquateTaggerSettings, INamedTagger>(s => new EquateTagger(s)),
        new M4BFillUpTagger(),
        settingsProvider.Build<IPrependSeriesToDescriptionTaggerSettings, INamedTagger>(s => new PrependSeriesToDescriptionTagger(s.PrependSeriesToDescription)),
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
    var script = "";
    var javaScriptApi = settingsProvider.Build<IScriptSettings, JavaScriptApi>(s =>
    {
        script = s.Scripts.Aggregate(script, (current, scr) => current + fs.File.ReadAllText(scr));
        return new JavaScriptApi(jint, fs, http, taggerComposite, s.ScriptTaggerParameters);
    }) ?? new JavaScriptApi();

    jint.SetValue("tone", javaScriptApi);
    jint.SetValue("console", scriptConsole);
    jint.Execute(script);
    return javaScriptApi;
});

// services.AddSingleton(_ => AnsiConsole.Console);
// services.AddLogging(builder => builder.AddSerilog(dispose: true));
// services.AddSingleton<ILogger>(_ => Log.Logger);


var app = new CommandApp(new TypeRegistrar(services));

app.Configure(config =>
{
    config.SetInterceptor(new CommandInterceptor(settingsProvider));
    config.UseStrictParsing();
    config.CaseSensitivity(CaseSensitivity.None);
    config.SetApplicationName("tone");
    config.SetApplicationVersion("0.0.9");
    config.ValidateExamples();
    config.AddCommand<DumpCommand>("dump")
        .WithDescription("dump metadata for files and directories (directories are traversed recursively)")
        .WithExample(new[] { "dump", "--help" })
        .WithExample(new[] { "dump", "input.mp3" })
        .WithExample(new[]
        {
            "dump", "audio-directory/", "--include-extension", "m4b", "--format", "ffmetadata",
            "--include-property",
            "title", "--include-property", "artist"
        })
        ;
    config.AddCommand<TagCommand>("tag")
        .WithDescription("tag files with metadata properties (directories are traversed recursively)")
        .WithExample(new[] { "tag", "--help" })
        .WithExample(new[] { "tag", "input.mp3", "--meta-title", "\"a title\"" })
        .WithExample(new[]
        {
            "tag", "--debug", "--auto-import=covers", "--meta-additional-field", "©st3=testing", "input.m4b",
            "--dry-run"
        })
        .WithExample(new[]
        {
            "tag", "--auto-import=covers", "--auto-import=chapters",
            "--path-pattern=\"audiobooks/%g/%a/%s/%p - %n.m4b\"",
            "--path-pattern=\"audiobooks/%g/%a/%z/%n.m4b\"",
            "audiobooks/", "--dry-run"
        })
        ;
    /*config.AddCommand<SplitCommand>("split")
        .WithDescription("split audio files")
        ;    
        */

    if (propagateExceptions)
    {
        config.PropagateExceptions();
    }
#if DEBUG
    config.ValidateExamples();
#endif
});
try
{
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