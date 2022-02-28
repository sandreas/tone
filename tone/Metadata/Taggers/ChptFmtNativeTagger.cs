using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using tone.Metadata.Parsers;
using static System.Array;

namespace tone.Metadata.Taggers;

public class ChptFmtNativeTagger : TaggerBase
{
    private readonly IFileSystem? _fs;
    private readonly ChptFmtNativeParser _parser;
    private readonly string _forceChapterFilename;

    public ChptFmtNativeTagger(IFileSystem? fileSystem, ChptFmtNativeParser parser, string forceChapterFilename="")
    {
        _fs = fileSystem;
        _parser = parser;
        _forceChapterFilename = forceChapterFilename;
    }

    public override void Update(IMetadata metadata)
    {
        var audioFile = _fs?.FileInfo.FromFileName(metadata.Path);
        if (audioFile == null)
        {
            return;
        }

        IEnumerable<IFileInfo> chaptersTxtFiles = Empty<IFileInfo>();
        if (_forceChapterFilename == "")
        {

            chaptersTxtFiles = _fs?.Directory.EnumerateFiles(audioFile.DirectoryName)
                .Select(f => _fs.FileInfo.FromFileName(f))
                .Where(f => f.Name.EndsWith("chapters.txt")).ToArray() ?? Empty<IFileInfo>();
        }
        else
        {
            var forcedFile = _fs?.FileInfo.FromFileName(_forceChapterFilename);
            chaptersTxtFiles = forcedFile == null ? Empty<IFileInfo>() : new[]{forcedFile} ;
        }

        if (!chaptersTxtFiles.Any())
        {
            return;
        }
        
        var preferredFileName = audioFile.Name[..audioFile.Extension.Length] + "chapters.txt";
        var preferredFile = chaptersTxtFiles.FirstOrDefault(f => f.Name == preferredFileName) ??
                            chaptersTxtFiles.First();
        using var stream = _fs?.File.OpenRead(preferredFile.FullName);
        if (stream == null)
        {
            return;
        }
        
        var parsedMeta = _parser.Parse(stream);
        TransferMetadataList(parsedMeta.Chapters, metadata.Chapters);
    }
}