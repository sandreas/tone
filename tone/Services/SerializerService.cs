using System.Diagnostics;
using System.Threading.Tasks;
using ATL;
using tone.Metadata;
using tone.Metadata.Formats;
using tone.Metadata.Serializers;

namespace tone.Services;
public enum SerializerFormat {
    Default,
    Json
}
public class SerializerService
{
    private readonly MetadataTextSerializer _text;

    public SerializerService(MetadataTextSerializer text)
    {
        _text = text;
    }

    public async Task<string> SerializeAsync(IMetadata metadata, SerializerFormat? format = null) => format switch
    {
        _ => await _text.SerializeAsync(metadata)
    };
}