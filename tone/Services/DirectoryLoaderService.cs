using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using Sandreas.Files;
using Serilog;

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
    private readonly FileWalker _fileWalker;

    public IFileSystem? FileSystem => _fileWalker?.FileSystem;
    
    public DirectoryLoaderService(FileWalker fileWalker)
    {
        _fileWalker = fileWalker;
    }

    public static IEnumerable<string> ComposeAudioExtensions(IEnumerable<string>? extensions)
    {
        return ComposeExtensions(extensions, SupportedAudioExtensions);
    }
    
    public static IEnumerable<string> ComposeExtensions(IEnumerable<string>? extensions, IEnumerable<string> fallbackExtensions)
    {
        var extensionList = extensions?.ToList() ?? new List<string>();
        return (!extensionList.Any() ? fallbackExtensions : extensionList).Select(e => "." + e);
    }

    private FileWalker WalkRecursive(string inputPath, FileWalker? fileWalker = null)
    {
        fileWalker ??= _fileWalker;
        return fileWalker.WalkRecursive(inputPath)
            .Catch((path, ex) =>
            {
                // _logger.Warning("{Path} could not be loaded: {Message}", path, ex.Message);
                return FileWalkerBehaviour.Default;
            });
    }
    
    public IEnumerable<IFileInfo> FindFilesByExtension(IEnumerable<string> inputPaths,IEnumerable<string> includeExtensions, FileWalker? fileWalker = null)
    {
        fileWalker ??= _fileWalker;
        var includeExtensionList = includeExtensions.ToList();
        foreach (var inputPath in inputPaths)
        {
            if (!fileWalker.IsDir(inputPath))
            {
                yield return fileWalker.FileSystem.FileInfo.FromFileName(inputPath);
                continue;
            }
            var files = FindFilesByExtension(inputPath, includeExtensionList, fileWalker);
            foreach (var file in files)
            {
                yield return file;
            }
        }
    }
    private IEnumerable<IFileInfo> FindFilesByExtension(string inputPath,IEnumerable<string> includeExtensions, FileWalker? fileWalker = null)
    {
        fileWalker ??= _fileWalker;
        return WalkRecursive(inputPath, fileWalker)
            .SelectFileInfo()
            .Where(f => !fileWalker.IsDir(f) && includeExtensions.Contains(f.Extension));
    }
    public IEnumerable<(string path, IEnumerable<IFileInfo>)> SeekFiles(IEnumerable<string> optionsInput,
        IEnumerable<string> includeExtensions, FileWalker? fileWalker = null)
    {
        fileWalker ??= _fileWalker;

        return optionsInput
            .Select(input => (
                input,
                FindFilesByExtension(input, includeExtensions, fileWalker)
            ));

    }
}