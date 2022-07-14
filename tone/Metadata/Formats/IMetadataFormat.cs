using System.IO;
using OperationResult;

namespace tone.Metadata.Formats;

public interface IMetadataFormat : IAsyncReader<Stream, Result<IMetadata, string>>,
    IAsyncWriter<IMetadata, Stream, Status<string>>
{
}