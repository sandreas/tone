using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using tone.Metadata;
using tone.Services;


namespace tone.Commands;

public class DumpCommand : AsyncCommand<DumpCommandSettings>
{
    private readonly SerializerService _serializerService;
    private readonly DirectoryLoaderService _dirLoader;
    private readonly SpectreConsoleService _console;

    public DumpCommand(SpectreConsoleService console, DirectoryLoaderService dirLoader,
        SerializerService serializerService)
    {
        _console = console;
        _dirLoader = dirLoader;
        _serializerService = serializerService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DumpCommandSettings settings)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(settings.IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(settings.Input, audioExtensions).ToArray();
        foreach (var file in inputFiles)
        {
            // ideas:
            // - prefix, separator, suffix (for json output? [ , ])
            // - JsonSerializer
            var track = new MetadataTrack(file);

            if (settings.IncludeProperties.Length > 0)
            {
                track.ClearProperties(settings.IncludeProperties);
            }

            var serializeResult = await _serializerService.SerializeAsync(track, settings.Format);
            _console.WriteLine(serializeResult);
        }

        return await Task.FromResult((int)ReturnCode.Success);
    }

    private void PerformDump(MetadataTrack track)
    {
        
    }
}