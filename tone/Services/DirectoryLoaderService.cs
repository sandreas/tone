using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Microsoft.Extensions.Logging;
using tone.Common.Io;

namespace tone.Services;

public class DirectoryLoaderService
{
    public static readonly IEnumerable<string> SupportedAudioExtensions = new[]
    {
        "aac",
        "aax",
        "aif",
        "aiff",
        "alac",
        "ape",
        "au",
        "caf",
        "flac",
        "m4a",
        "m4b",
        "m4p",
        "m4r",
        "mka",
        "mp2",
        "mp3",
        "mp4",
        "mpa",
        "rif",
        "oga",
        "ogg",
        "wav",
        "wma",        
    };
    private readonly ILogger<DirectoryLoaderService> _logger;
    private readonly FileWalker _fileWalker;

    public DirectoryLoaderService(ILogger<DirectoryLoaderService> logger, FileWalker fileWalker)
    {
        _logger = logger;
        _fileWalker = fileWalker;
    }

    public static IEnumerable<string> ComposeAudioExtensions(IEnumerable<string> extensions)
    {
        return ComposeExtensions(extensions, SupportedAudioExtensions);
    }
    
    public static IEnumerable<string> ComposeExtensions(IEnumerable<string> extensions, IEnumerable<string> fallbackExtensions)
    {
        var extensionList = extensions.ToList();
        return (!extensionList.Any() ? fallbackExtensions : extensionList).Select(e => "." + e);
    }
    
    public IEnumerable<(string path, IEnumerable<IFileInfo>)> SeekFiles(IEnumerable<string> optionsInput,
        IEnumerable<string> includeExtensions, FileWalker? fileWalker = null)
    {
        fileWalker ??= _fileWalker;

        return optionsInput
            .Select(input =>
            {
                return (
                    input,
                    fileWalker.WalkRecursive(input)
                        .Catch((path, ex) =>
                        {
                            _logger.LogWarning("{Path} could not be loaded: {Message}", path, ex.Message);
                            return FileWalkerBehaviour.ContinueOnException;
                        })
                        .SelectFileInfo()
                        .Where(f => !fileWalker.IsDir(f) && includeExtensions.Contains(f.Extension))
                );
            });
        /*
        foreach (var input in optionsInput)
        {
            var files = fileWalker.Walk(input).SelectFileInfo()
                .Where(f => !fileWalker.IsDir(f) && includeExtensions.Contains(f.Extension));
            foreach (var file in files)
            {
                yield return file;
            }
        }
        */
    }
}