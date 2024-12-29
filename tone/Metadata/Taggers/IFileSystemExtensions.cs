using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace tone.Metadata.Taggers;


public static class FileSystemExtensions
{
    
    public static IDirectoryInfo GetContainingDirectory(this IFileSystem fs, string path) {
        var attr = fs.File.GetAttributes(path);
        if ((attr & FileAttributes.Directory) != FileAttributes.Directory) {
            path = fs.Path.GetDirectoryName(path) ?? path;
        }

        return fs.DirectoryInfo.New(path);
    }
    
    public static IEnumerable<IFileInfo> FindMatchingFiles(this IFileSystem fs, string path, string fileNameSuffix, string fileNamePrefix="")
    {
        var result = new List<IFileInfo>();
        var attr = fs.File.GetAttributes(path);
        if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
        {
            var newFileNamePrefix = fileNamePrefix;
            if(newFileNamePrefix == "")
            {
                newFileNamePrefix = fs.Path.GetFileNameWithoutExtension(path) + ".";
            }
            path = fs.Path.GetDirectoryName(path) ?? path;
            result.AddRange(fs.Directory.EnumerateFiles(path).Where(f => f.StartsWith(newFileNamePrefix) && f.EndsWith(fileNameSuffix)).Select(f => fs.FileInfo.New(f)));
            if (result.Count > 0)
            {
                return result;
            }
        }
        
        return fs.Directory.EnumerateFiles(path).Where(f => (fileNamePrefix == "" || f.StartsWith(fileNamePrefix)) && f.EndsWith(fileNameSuffix)).Select(f => fs.FileInfo.New(f));
    }
}