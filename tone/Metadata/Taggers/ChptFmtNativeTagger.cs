using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Metadata.Formats;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class ChptFmtNativeTagger : AbstractFilesystemTagger, INamedTagger
{
    public static readonly string DefaultFileSuffix = "chapters.txt";
    public string Name => nameof(ChptFmtNativeTagger);
    public ChptFmtNativeTagger(IFileSystem? fileSystem, ChptFmtNativeMetadataFormat parser, string forcedImportFilename = "", bool autoImport = false) : base(fileSystem, parser, forcedImportFilename, autoImport)
    {
    }

    protected override string? BuildPreferredFileName(IFileInfo audioFile) => ConcatPreferredFileName(audioFile, DefaultFileSuffix);

    protected override bool FilterCallback(IFileInfo f) => f.Name.EndsWith(DefaultFileSuffix);

    protected override Status<string> TransferPropertiesCallback(IMetadata parsedMetaValue, IMetadata metadata)
    {
        TransferMetadataList(parsedMetaValue.Chapters, metadata.Chapters);

        var lastChapter = metadata.Chapters.LastOrDefault();
        if(lastChapter is { EndTime: 0 } && metadata.TotalDuration.TotalMilliseconds > lastChapter.StartTime)
        {
            lastChapter.EndTime = (uint)metadata.TotalDuration.TotalMilliseconds;
        }

        return Ok();
    }
    
    private static void TransferMetadataList<T>(IList<T>? source, IList<T>? destination) where T : class
    {
        if (source == null || source.Count == 0 || destination == null)
        {
            return;
        }

        destination.Clear();
        foreach (var s in source)
        {
            destination.Add(s);
        }
    }
}