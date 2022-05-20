using System;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Sandreas.Files;
using Spectre.Console;
using Spectre.Console.Cli;
using tone;
using tone.Commands;
using tone.DependencyInjection;
using tone.Metadata.Formats;
using tone.Metadata.Serializers;
using tone.Services;

var propagateExceptions = args.Contains("--debug");

var services = new ServiceCollection();

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

// services.AddSingleton(s => new MetadataTagger(s.GetRequiredService<TagSettingsBase>()));
// services.AddSingleton<TagService>();
// services.AddSingleton<TaggerComposite>();

// services.AddSingleton<TaggerComposite>();

//
// services.AddSingleton<TagCommand>();
// services.AddSingleton<DumpCommand>();
// // services.AddSingleton<TagService>();
// // services.AddSingleton<JobBuilderService>();
// services.AddLogging(builder => builder.AddSerilog(dispose: true));
// services.AddSingleton<ILogger>(_ => Log.Logger);

// services.AddSingleton(_ => AnsiConsole.Console);
// services.AddSingleton<DumpCommand>();

var app = new CommandApp(new TypeRegistrar(services));

app.Configure(config =>
{
    
    // config.Settings;
    config.UseStrictParsing();
    config.CaseSensitivity(CaseSensitivity.None);
    config.SetApplicationName("tone");
    config.SetApplicationVersion("0.0.1");
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