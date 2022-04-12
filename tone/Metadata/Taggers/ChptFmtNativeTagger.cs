using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using OperationResult;
using tone.Metadata.Formats;
using static System.Array;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class ChptFmtNativeTagger : TaggerBase
{
    private readonly IFileSystem? _fs;
    private readonly ChptFmtNativeMetadataFormat _parser;
    private readonly string _forceChapterFilename;

    public ChptFmtNativeTagger(IFileSystem? fileSystem, ChptFmtNativeMetadataFormat parser,
        string forceChapterFilename = "")
    {
        _fs = fileSystem;
        _parser = parser;
        _forceChapterFilename = forceChapterFilename;
    }

    public override async Task<Status<string>> Update(IMetadata metadata)
    {
        var audioFile = _fs?.FileInfo.FromFileName(metadata.Path);
        if (audioFile == null)
        {
            return Error($"Could not create fileInfo for file {metadata.Path}");
        }

        IEnumerable<IFileInfo> chaptersTxtFiles;
        if (_forceChapterFilename == "")
        {
            chaptersTxtFiles = _fs?.Directory.EnumerateFiles(audioFile.DirectoryName)
                .Select(f => _fs.FileInfo.FromFileName(f))
                .Where(f => f.Name.EndsWith("chapters.txt")).ToArray() ?? Empty<IFileInfo>();
        }
        else
        {
            var forcedFile = _fs?.FileInfo.FromFileName(_forceChapterFilename);
            chaptersTxtFiles = forcedFile == null ? Empty<IFileInfo>() : new[] { forcedFile };
        }

        if (!chaptersTxtFiles.Any())
        {
            return _forceChapterFilename == "" ? Ok() : Error($"Could not find any chapter files in {metadata.Path}");
        }

        var preferredFileName = audioFile.Name[..audioFile.Extension.Length] + "chapters.txt";
        var preferredFile = chaptersTxtFiles.FirstOrDefault(f => f.Name == preferredFileName) ??
                            chaptersTxtFiles.First();
        await using var stream = _fs?.File.OpenRead(preferredFile.FullName);
        if (stream == null)
        {
            return Error($"Could not open file ${preferredFile.FullName}");
        }

        var parsedMeta = await _parser.ReadAsync(stream);
        if (!parsedMeta)
        {
            return Error(parsedMeta.Error);
        }

        TransferMetadataList(parsedMeta.Value.Chapters, metadata.Chapters);
        return Ok();
    }
}