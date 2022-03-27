using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using tone.Metadata;
using tone.Services;
using static System.Array;

namespace tone.Commands;

public class DumpSettings : CommandSettings
{
    [Description("Input files or folders")]
    [CommandArgument(0, "[input]")]
    public string[] Input { get; set; } = Empty<string>();

    [CommandOption("--include-extensions")]
    public string[] IncludeExtensions { get; set; } = Empty<string>();
}

public class DumpCommand : AsyncCommand<DumpSettings>
{
    private readonly SpectreConsoleService _console;
    private readonly SerializerService _serializerService;
    private readonly DirectoryLoaderService _dirLoader;

    // public DumpCommand(IAnsiConsole console, DirectoryLoaderService dirLoader, SerializerService serializerService)
    // {
    //     _console = console;
    //     _serializerService = serializerService;
    //     _dirLoader = dirLoader;
    // }
    public DumpCommand(SpectreConsoleService console, DirectoryLoaderService dirLoader,
        SerializerService serializerService)
    {
        _console = console;
        _dirLoader = dirLoader;
        _serializerService = serializerService;
    }



    

    public override async Task<int> ExecuteAsync(CommandContext context, DumpSettings settings)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(settings.IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(settings.Input, audioExtensions).ToArray();
        foreach (var file in inputFiles)
        {
            // ideas:
            // - prefix, separator, suffix (for json output? [ , ])
            // - JsonSerializer
            // - FfmetadataSerializer
            // - SerializerService with Format - serializerService.Serialize(track, Format.Json);
            var track = new MetadataTrack(file);
            
            _ = await _serializerService.SerializeAsync(track, SerializerFormat.SpectreConsole);
            // await console.Output.WriteLineAsync(output);
        }
        return await Task.FromResult(0);
    }
    // private readonly SerializerService _serializerService;
    // private readonly DirectoryLoaderService _dirLoader;
    //
    // [CommandParameter(0, Description = "Input files and folders")]
    // public IReadOnlyList<string> Input { get; init; } = new List<string>();
    //
    // [CliFx.Attributes.CommandOption("include-extensions")]
    // public IReadOnlyList<string> IncludeExtensions { get; init; } = new List<string>();
    //
    // [CliFx.Attributes.CommandOption("short")] public bool ShortDump { get; init; } = false;
    //
    // public DumpCommand(DirectoryLoaderService dirLoader, SerializerService serializerService)
    // {
    //     _serializerService = serializerService;
    //     _dirLoader = dirLoader;
    // }
    //
    // public async ValueTask ExecuteAsync(IConsole console)
    // {
    //     var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(IncludeExtensions);
    //     var inputFiles = _dirLoader.FindFilesByExtension(Input, audioExtensions).ToArray();
    //     foreach (var file in inputFiles)
    //     {
    //         // ideas:
    //         // - prefix, separator, suffix (for json output? [ , ])
    //         // - JsonSerializer
    //         // - FfmetadataSerializer
    //         // - SerializerService with Format - serializerService.Serialize(track, Format.Json);
    //         var track = new MetadataTrack(file);
    //         var output = await _serializerService.SerializeAsync(track, SerializerFormat.SpectreConsole);
    //         await console.Output.WriteLineAsync(output);
    //     }
    // }
}