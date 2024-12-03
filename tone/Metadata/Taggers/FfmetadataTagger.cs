using System.IO.Abstractions;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Metadata.Formats;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class FfmetadataTagger : AbstractFilesystemTagger, INamedTagger
{
    public static readonly string DefaultFileSuffix = "ffmetadata.txt";

    public string Name => nameof(FfmetadataTagger);
    public FfmetadataTagger(IFileSystem? fileSystem, FfmetadataFormat parser, string forcedImportFilename = "", bool autoImport = false) : base(fileSystem, parser, forcedImportFilename, autoImport)
    {
    }

    protected override string? BuildPreferredFileName(IFileInfo audioFile) => ConcatPreferredFileName(audioFile, DefaultFileSuffix);

    protected override bool FilterCallback(IFileInfo f) => f.Name.EndsWith(DefaultFileSuffix) || f.Name.EndsWith(".ffmetadata");

    protected override Status<string> TransferPropertiesCallback(IMetadata parsedMetaValue, IMetadata metadata)
    {
        metadata.OverwritePropertiesWhenNotEmpty(parsedMetaValue);
        return Ok();
    }
}