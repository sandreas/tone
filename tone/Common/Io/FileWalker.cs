using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace tone.Common.Io;

[Flags]
public enum FileWalkerOptions
{
    Default = 1 << 0,
    Recursive = 1 << 1,
}

[Flags]
public enum FileWalkerBehaviour
{
    Default = 0,
    BreakOnException = 1 << 0
}

// https://stackoverflow.com/questions/18982108/making-a-custom-class-iqueryable
public class FileWalker : IEnumerable<string>
{
    private readonly FileSystem _fs;
    private string _path = "";
    private Func<string, Exception, FileWalkerBehaviour> _exceptionHandler;
    private FileWalkerOptions _options = FileWalkerOptions.Default;

    public FileWalker(FileSystem fs, Func<string, Exception, FileWalkerBehaviour>? exceptionHandler = null)
    {
        _fs = fs;
        _exceptionHandler = exceptionHandler ?? ((_, _) => FileWalkerBehaviour.Default);
    }

    public bool IsDir(IFileInfo f)
    {
        return f.Attributes.HasFlag(FileAttributes.Directory);
    }
    
    public FileWalker Walk(string path)
    {
        _options &= ~FileWalkerOptions.Recursive;
        _path = path;
        return this;
    }
    
    public FileWalker WalkRecursive(string path)
    {
        _options |= FileWalkerOptions.Recursive;
        _path = path;
        return this;
    }

    public IEnumerable<IFileInfo> SelectFileInfo()
    {
        return SelectWithFileSystem((p, fs) => fs.FileInfo.FromFileName(p));
    }
    public IEnumerable<TReturn> SelectWithFileSystem<TReturn> (Func<string, IFileSystem, TReturn> func)
    {
        return this.Select(p => func(p,_fs));
    }

    public FileWalker Catch(Func<string, Exception, FileWalkerBehaviour> exceptionHandler)
    {
        _exceptionHandler = exceptionHandler;
        return this;
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
        var e = GetEnumerator();
        while (e.MoveNext())
        {
            yield return (string)(e.Current ?? "");
        }
    }

    public IEnumerator GetEnumerator()
    {
        var currentPath = _path;
        var pending = new Stack<string>();
        pending.Push(currentPath);
        while (pending.Count != 0)
        {
            currentPath = pending.Pop();
            IEnumerable<string> tmp;

            try
            {
                tmp = _fs.Directory.EnumerateFiles(currentPath);
            }
            catch (Exception e)
            {
                if (_exceptionHandler(currentPath, e) == FileWalkerBehaviour.BreakOnException)
                {
                    break;
                }
                continue;
            }
            
            yield return currentPath;

            var dirs = _fs.Directory.EnumerateDirectories(currentPath).ToArray();
            if (_options.HasFlag(FileWalkerOptions.Recursive))
            {
                dirs.AsParallel().ForAll(pending.Push);
            }

            foreach (var item in tmp.Concat(dirs))
            {
                yield return item;
            }
        }
    }
}