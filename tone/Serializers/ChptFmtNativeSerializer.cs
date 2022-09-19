using System.IO;
using System.Threading.Tasks;
using Sandreas.AudioMetadata;
using tone.Common.Extensions.Stream;
using tone.Metadata.Formats;

namespace tone.Serializers;

public class ChptFmtNativeSerializer: IMetadataSerializer
{
    private readonly ChptFmtNativeMetadataFormat _chptFmtNative;

    public ChptFmtNativeSerializer(ChptFmtNativeMetadataFormat chptFmtNative)
    {
        _chptFmtNative = chptFmtNative;
    }
    
    public async Task<string> SerializeAsync(IMetadata metadata)
    {
        var output = new MemoryStream();
        var result = await _chptFmtNative.WriteAsync(metadata, output);
        return result ? output.StreamToString() : "";
    }
}