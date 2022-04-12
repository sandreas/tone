using System.IO;
using OperationResult;
using tone.Common.Data;

namespace tone.Metadata.Formats;

public interface IMetadataFormat : IAsyncReader<Stream, Result<IMetadata, string>>,
    IAsyncWriter<IMetadata, Stream, Status<string>>
{
}