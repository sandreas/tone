using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using GrokNet;
using Sandreas.Files;
using Serilog;
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
            .Catch((_, _) => FileWalkerBehaviour.Default);
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

    public IList<AudioBookPackage> BuildPackages(IEnumerable<IFileInfo> files, PathPatternMatcher pathMatcher)
    {
        /*
         * class AudioBookPackage
         * - enum type: dir/file
         * - string baseDir: <containing dir>
         * - list files: IList<IFileInfo>
         * - grok pattern: Grok
         * - GrokResult: matches
         * - FindCovers?
         * - FindChapters
         */
        var fileMatches = new Dictionary<string, IList<IFileInfo>>();
        var dirMatches = new Dictionary<string, IList<IFileInfo>>();
        var packages = new List<AudioBookPackage>();
        
        
        var filesArray = files.ToArray();
        
        // group files by matching path pattern
        foreach (var file in filesArray)
        {
            var patternAsString = "";
            var grokResult = new GrokResult(new List<GrokItem>());
            if (!pathMatcher.TryMatchSinglePattern(file.FullName, out var result, (pattern, grokPattern, grokPatternResult) =>
                {
                    patternAsString = pattern;
                    grokResult = grokPatternResult;
                }))
            {
                continue;
            }

            var identifier = string.Join(",",grokResult.Select(r => r.Key+"="+r.Value));
            var type = (file.Attributes & FileAttributes.Directory) == 0
                ? AudioBookPackageType.File
                : AudioBookPackageType.Directory;

            
            var existingDirectoryPackage = packages.FirstOrDefault(p => p.Id == identifier && p.Type == AudioBookPackageType.Directory);
            if (existingDirectoryPackage == null)
            {
                packages.Add(new AudioBookPackage
                {
                    Type = type,
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
            
            /*
            if()
            {

                var package = packages.FirstOrDefault(p =>
                    p.PatternString == patternAsString && p.Type == AudioBookPackageType.Directory);
                
                if (package == null)
                {
                    packages.Add(new AudioBookPackage()
                    {
                        Type=AudioBookPackageType.Directory,
                        PatternString = patternAsString,
                        BaseDirectory = null,
                        Files = new List<IFileInfo>()
                        {
                            file
                        }
                    });
                }
                else
                {
                    package.Files.Add(file);
                }
                continue;
            }
            
            var filePackage = packages.FirstOrDefault(p =>
                p.PatternString == patternAsString && p.Type == AudioBookPackageType.File);
            
            if (filePackage)
            {
                fileMatches[patternAsString] = new List<IFileInfo>();                    
            }
            fileMatches[patternAsString].Add(file);
            */
        }


        foreach (var package in packages)
        {
            if (package.Files.Count == 0)
            {
                continue;
            }
            if (package.Type == AudioBookPackageType.Directory)
            {
                package.BaseDirectory = package.Files.MinBy(f => f.FullName.Length)?.Directory;
            }

            if (package.Type == AudioBookPackageType.File)
            {
                package.BaseDirectory = package.Files.MinBy(f => f.FullName.Length)?.Directory;
            }
        }

        
        /*
        // reorder these groups to use the containing path as key (shortest f.Name)
        var patternGroups = new Dictionary<string, Dictionary<string, IList<IFileInfo>>>();
        foreach (var (key, value) in dirMatches)
        {
            // find the shortest match for a pattern
            var shortest = value.MinBy(s => s.FullName.Length);
            if (shortest == null)
            {
                continue;
            }
            if(!patternGroups.ContainsKey(key))
            {
                patternGroups[key] = new Dictionary<string, IList<IFileInfo>>();
            }

            patternGroups[key][shortest.FullName] = value;
        }
        
        foreach(var (key, value) in fileMatches){
            foreach(var f in value){
                if(!patternGroups.ContainsKey(key))
                {
                    patternGroups[key] = new Dictionary<string, IList<IFileInfo>>();
                }

                patternGroups[key][f.FullName] = new List<IFileInfo> {f};
            }  
        }  
        
        

        // key: containing/shortest path
        // value: list of all files in it that where found by the iterator
        return patternGroups;
        */

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