using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Sandreas.Files;
using Spectre.Console;
using Spectre.Console.Cli;
using tone.Commands;
using tone.DependencyInjection;
using tone.Metadata.Formats;
using tone.Metadata.Serializers;
using tone.Services;


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
//
// services.AddSingleton<TagCommand>();
// services.AddSingleton<DumpCommand>();
// // services.AddSingleton<TagService>();
// // services.AddSingleton<JobBuilderService>();
// services.AddLogging(builder => builder.AddSerilog(dispose: true));
// services.AddSingleton<ILogger>(_ => Log.Logger);

services.AddSingleton(_ => new SpectreConsoleService());
// services.AddSingleton(_ => AnsiConsole.Console);
// services.AddSingleton<DumpCommand>();

var app = new CommandApp(new TypeRegistrar(services));

app.Configure(config =>
{
    config.CaseSensitivity(CaseSensitivity.None);
    config.SetApplicationName("tone");
    config.ValidateExamples();
    config.AddCommand<DumpCommand>("dump")
        .WithDescription("dump metadata for files and directories (directories are traversed recursively)")
        .WithExample(new[] { "dump", "input.mp3" });
    config.AddCommand<TagCommand>("tag")
        .WithDescription("tag files with metadata properties (directories are traversed recursively)")
        .WithExample(new[] { "tag", "input.mp3", "--meta-title", "a title"});
});
try
{
    return await app.RunAsync(args).ConfigureAwait(false);
}
catch (Exception e)
{
    AnsiConsole.WriteException(e);
    return 1;
}
