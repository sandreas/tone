using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Sandreas.Files;
using tone.Services;
using Serilog;
using tone.Common.Extensions;

namespace tone.Commands;

[Command("tag")]
public class TagCommand: ICommand
{
    private readonly ILogger _logger;
    private readonly FileWalker _fileWalker;
    private readonly DirectoryLoaderService _dirLoader;

    [CommandOption("input", 'i')]
    public IReadOnlyList<string> Input { get; init; }

    [CommandOption("include-extensions")] public IReadOnlyList<string> IncludeExtensions { get; init; }
    
    
    /*
    private readonly ILogger<ICommand> _logger;
    private readonly FileWalker _fileWalker;
    private readonly DirectoryLoaderService _dirLoader;

    public TagCommand(ILogger<ICommand> logger, FileWalker fileWalker, DirectoryLoaderService dirLoader)
    {
        _logger = logger;
        _fileWalker = fileWalker;
        _dirLoader = dirLoader;
    }


    public async Task<int> ExecuteAsync(TagOptions options)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(options.IncludeExtensions);
        var inputFiles = _dirLoader.SeekFiles(options.Input, audioExtensions).ToList();

        foreach (var file in inputFiles)
        {
            try
            {
                var track = new Track(file.path);
                track.Album = options.Album ?? track.Album;
                track.Artist = options.Artist ?? track.Artist;
                if (!track.Save())
                {
                    _logger.LogWarning("Could not save tags for {FilePath}", file.path);
                };
            }
            catch (Exception e)
            {
                _logger.LogError("Could not save tag for {FilePath}, exception: {ExceptionMessage}", file.path, e.Message);
            }

        }
        return await Task.FromResult(0);
    }

    private void SetTagValue(string? optionsAlbum, out string trackAlbum)
    {
        throw new NotImplementedException();
    }
    */
    public TagCommand(ILogger logger, FileWalker fileWalker, DirectoryLoaderService dirLoader)
    {
        _logger = logger;
        _fileWalker = fileWalker;
        _dirLoader = dirLoader;
    }
    public async ValueTask ExecuteAsync(IConsole console)
    {
        var audioExtensions = DirectoryLoaderService.ComposeAudioExtensions(IncludeExtensions);
        var inputFiles = _dirLoader.FindFilesByExtension(Input, audioExtensions);

        console.WriteErrorLine(inputFiles.Count().ToString());
        
        // await console.Output.WriteLineAsync("hello tag command");
        // _logger.Error("Error testing from tag command");
        

    }
}