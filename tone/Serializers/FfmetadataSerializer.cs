using System.IO;
using System.Threading.Tasks;
using Sandreas.AudioMetadata;
using tone.Common.Extensions.Stream;
using tone.Metadata.Formats;

namespace tone.Serializers;

public class FfmetadataSerializer : IMetadataSerializer
{
    private readonly FfmetadataFormat _ffmeta;

    public FfmetadataSerializer(FfmetadataFormat ffmeta)
    {
        _ffmeta = ffmeta;
    }

    public async Task<string> SerializeAsync(IMetadata metadata)
    {
        var output = new MemoryStream();
        var result = await _ffmeta.WriteAsync(metadata, output);
        return result ? output.StreamToString() : "";
    }
}