using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Enumeration;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using ATL;
using Microsoft.Extensions.Logging;
using tone.Common.Io;
using tone.Options;
using tone.Services;

namespace tone.Commands;

public class TagCommand: ICommand<TagOptions>
{
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
}