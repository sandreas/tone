using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using tone.Metadata;
using tone.Services;

namespace tone.Commands;

[Command("dump")]
public class DumpCommand : ICommand
{
    private readonly SerializerService _serializerService;
    private readonly DirectoryLoaderService _dirLoader;
    
    [CommandParameter(0, Description = "Input files and folders")]
    public IReadOnlyList<string> Input { get; init; } = new List<string>();

    [CommandOption("include-extensions")]
    public IReadOnlyList<string> IncludeExtensions { get; init; } = new List<string>();

    [CommandOption("short")] public bool ShortDump { get; init; } = false;

    public DumpCommand(DirectoryLoaderService dirLoader, SerializerService serializerService)
    {
        _serializerService = serializerService;
        _dirLoader = dirLoader;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(Input, audioExtensions).ToArray();
        foreach (var file in inputFiles)
        {
            // ideas:
            // - prefix, separator, suffix (for json output? [ , ])
            // - JsonSerializer
            // - FfmetadataSerializer
            // - SerializerService with Format - serializerService.Serialize(track, Format.Json);
            var track = new MetadataTrack(file);
            var output = await _serializerService.SerializeAsync(track);
            await console.Output.WriteLineAsync(output);
        }
    }
}


