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


    [CommandOption("--format")] public SerializerFormat Format { get; set; } = SerializerFormat.Default;

    [CommandOption("--include-extensions")]
    public string[] IncludeExtensions { get; set; } = Empty<string>();
}

public class DumpCommand : AsyncCommand<DumpSettings>
{
    private readonly SerializerService _serializerService;
    private readonly DirectoryLoaderService _dirLoader;
    private readonly SpectreConsoleService _console;

    public DumpCommand(SpectreConsoleService console,DirectoryLoaderService dirLoader,
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
            var track = new MetadataTrack(file);
            var serializeResult = await _serializerService.SerializeAsync(track, settings.Format);
            _console.WriteLine(serializeResult);
        }
        return await Task.FromResult(0);
    }
}