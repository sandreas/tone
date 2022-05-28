using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using GrokNet;
using Microsoft.Extensions.DependencyInjection;
using Sandreas.Files;
using Spectre.Console;
using Spectre.Console.Cli;
using tone;
using tone.Commands;
using tone.Commands.Settings.Interfaces;
using tone.DependencyInjection;
using tone.Interceptors;
using tone.Matchers;
using tone.Metadata;
using tone.Metadata.Formats;
using tone.Metadata.Serializers;
using tone.Metadata.Taggers;
using tone.Services;

var propagateExceptions = args.Contains("--debug");

var settingsProvider = new CommandSettingsProvider();

var services = new ServiceCollection();

services.AddSingleton(_ => settingsProvider);
services.AddSingleton<StartupErrorService>();

// services.AddTransient<StringWriter>();
services.AddSingleton<FileSystem>();
services.AddSingleton<FileWalker>();
services.AddSingleton<DirectoryLoaderService>();
services.AddSingleton<GrokPatternService>();

services.AddSingleton<ChptFmtNativeMetadataFormat>();
services.AddSingleton<FfmetadataFormat>();

services.AddSingleton<FfmetadataSerializer>();
services.AddSingleton<SpectreConsoleSerializer>();
services.AddSingleton<SerializerService>();

services.AddSingleton(_ => new SpectreConsoleService());
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
            startupErrorsService.Errors.Add((ReturnCode.GeneralError, "Could not parse `--path-pattern`: " + grokDefinitions.Error));
        }  
    }
    
    return new PathPatternMatcher(pathPatterns);
});

services.AddSingleton(sp =>
{
    var fs = sp.GetRequiredService<FileSystem>();
    var pathMatcher = sp.GetRequiredService<PathPatternMatcher>();
    var chapterFormat = sp.GetRequiredService<ChptFmtNativeMetadataFormat>();
    var taggers = new[]
    {
        settingsProvider.Build<IMetadata, ITagger>(s => new MetadataTagger(s)),
        settingsProvider.Build<ICoverTaggerSettings, ITagger>(s => new CoverTagger(fs, s)),
        settingsProvider.Build<IPathPatternSettings, ITagger>(_ => new PathPatternTagger(pathMatcher)),
        settingsProvider.Build<IRemoveAdditionalFieldsSettings, ITagger>(s => new AdditionalFieldsRemoveTagger(s)),
        settingsProvider.Build<IChptFmtNativeTaggerSettings, ITagger>(s => new ChptFmtNativeTagger(fs, chapterFormat, s.ImportChaptersFile, s.AutoImportChapters)),
        settingsProvider.Build<IEquateTaggerSettings, ITagger>(s => new EquateTagger(s)),
        new M4BFillUpTagger()
    }.Where(t => t!= null).Select(e => e!).ToArray();

    return new TaggerComposite(taggers);
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
    config.SetApplicationVersion("0.0.2");
    config.ValidateExamples();
    config.AddCommand<DumpCommand>("dump")
        .WithDescription("dump metadata for files and directories (directories are traversed recursively)")
        .WithExample(new[] { "dump", "--help" })
        .WithExample(new[] { "dump", "input.mp3" })
        .WithExample(new[] { "dump", "audio-directory/", "--include-extension", "m4b", "--format", "ffmetadata", "--include-property", "title", "--include-property", "artist" })
        ;
    config.AddCommand<TagCommand>("tag")
        .WithDescription("tag files with metadata properties (directories are traversed recursively)")
        .WithExample(new[] { "tag", "--help" })
        .WithExample(new[] { "tag", "input.mp3", "--meta-title", "\"a title\"" })
        .WithExample(new[] { "tag", "--debug", "--auto-import=covers","--meta-additional-field", "©st3=testing", "input.m4b", "--dry-run"})
        .WithExample(new[] { "tag", "--auto-import=covers", "--auto-import=chapters",
            "--path-pattern=\"audiobooks/%g/%a/%s/%p - %n.m4b\"",
            "--path-pattern=\"audiobooks/%g/%a/%z/%n.m4b\"", 
            "audiobooks/", "--dry-run"})
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