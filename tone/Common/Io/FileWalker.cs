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
    IncludeSelf = 1 << 1,
    Recursive = 1 << 2,
}

public enum FileWalkerBehaviour
{
    ContinueOnException = 1 << 0,
    BreakOnException = 1 << 1,
}

// https://stackoverflow.com/questions/18982108/making-a-custom-class-iqueryable
public class FileWalker : IEnumerable<string>
{
    private readonly FileSystem _fs;
    private Func<string, Exception, FileWalkerBehaviour> _exceptionHandler;
    private string _path = "";
    private FileWalkerOptions _options = FileWalkerOptions.IncludeSelf;

    public FileWalker(FileSystem fs, Func<string, Exception, FileWalkerBehaviour>? exceptionHandler = null)
    {
        _fs = fs;
        _exceptionHandler = exceptionHandler ?? ((_, _) => FileWalkerBehaviour.ContinueOnException);
    }

    /*
    public static FileWalker WalkRecursive(FileSystem fs, string path)
    {
        return new FileWalker(fs).WalkRecursive(path);
    }
    */

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

            // todo: check if this would work? => maybe not even necessary
            // if (currentPath != _path || _flags.HasFlag(FileWalkerFlags.IncludeSelf))
            // {
            yield return currentPath;
            // }
            
            // todo: check if this would work? Improve performance (get rid of additional if)
            if (_options.HasFlag(FileWalkerOptions.Recursive))
            {
                _fs.Directory.EnumerateDirectories(currentPath).AsParallel().ForAll(pending.Push);
            } else {
                tmp = tmp.Concat(_fs.Directory.EnumerateDirectories(currentPath).AsParallel());
            }
    
            foreach (var item in tmp)
            {
                yield return item;
            }
        }
    }
}