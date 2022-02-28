using System.IO;
using OperationResult;
using tone.Common.Data;

namespace tone.Metadata.Format;

public interface IMetadataFormat: IAsyncReader<Stream, Result<IMetadata, string>>, IAsyncWriter<IMetadata, Stream, Status<string>>
{
    
}