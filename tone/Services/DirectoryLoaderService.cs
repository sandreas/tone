using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using GrokNet;
using Sandreas.Files;
using tone.Matchers;

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

    public IFileSystem FileSystem => _fileWalker.FileSystem;
    
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
            .Catch((_, _) => FileWalkerBehaviour.Default);
    }
    
    public FileWalker ListPath(string path)
    {
        return _fileWalker.Walk(path);
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

    public IList<AudioBookPackage> BuildPackages(IEnumerable<IFileInfo> files, PathPatternMatcher pathMatcher,
        string[] inputPaths)
    {
        var packages = new List<AudioBookPackage>();
        var filesArray = files.ToArray();

        if (!pathMatcher.HasPatterns)
        {
            var noPatternPackages = new List<AudioBookPackage>();
            if (inputPaths.Length == 0)
            {
                noPatternPackages.Add(new AudioBookPackage
                {
                    Files = filesArray
                });
                return noPatternPackages;
            }

            var sortedInputPaths = inputPaths.OrderByDescending(s => s.Length).ToArray();
            var noBasePathPackage = new AudioBookPackage(); 
            foreach (var file in filesArray)
            {
                var basePath = sortedInputPaths.FirstOrDefault(i => file.FullName.StartsWith(i));
                if (basePath == null)
                {
                    noBasePathPackage.Files.Add(file);
                    continue;
                }
                
                var basePathPackage = noPatternPackages.FirstOrDefault(p => p.BaseDirectory?.FullName == basePath);
                if (basePathPackage == null)
                {
                    noPatternPackages.Add(new AudioBookPackage()
                    {
                        BaseDirectory = FileSystem.DirectoryInfo.FromDirectoryName(basePath),
                        Files = new List<IFileInfo>()
                        {
                            file
                        }
                    });
                }
                else
                {
                    basePathPackage.Files.Add(file);
                }
            }

            if (noBasePathPackage.Files.Count > 0)
            {
                noPatternPackages.Add(noBasePathPackage);
            }

            return noPatternPackages;
        }
        
        // group files by matching path pattern
        foreach (var file in filesArray)
        {
            var grokResult = new GrokResult(new List<GrokItem>());
            if (!pathMatcher.TryMatchSinglePattern(file.FullName, out var grokPatternContainer, (_, _, grokPatternResult) =>
                {
                    grokResult = grokPatternResult;
                }))
            {
                continue;
            }

            // find baseDir by going up until the pattern does not match any more
            var grokPattern = grokPatternContainer.patternAsString;
            var baseDir = file.Directory;
            while(pathMatcher.TryMatchSinglePattern(baseDir.Parent.FullName, out var grokPatternContainer2) && grokPatternContainer2.patternAsString == grokPattern)
            {
                baseDir = baseDir.Parent;
                if(baseDir.Root.FullName == baseDir.FullName)
                {
                    break;
                }        
            }

            var identifier = string.Join(",",grokResult.Select(r => r.Key+"="+r.Value));
            
            var existingDirectoryPackage = packages.FirstOrDefault(p => p.Id == identifier && p.BaseDirectory?.FullName == baseDir.FullName);
            if (existingDirectoryPackage == null)
            {
                packages.Add(new AudioBookPackage
                {
                    Id = identifier,
                    BaseDirectory = baseDir,
                    Files = new List<IFileInfo>
                    {
                        file
                    },
                    Matches = grokResult
                });
            }
            else
            {
                existingDirectoryPackage.Files.Add(file);
            }
        }

        return packages;
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