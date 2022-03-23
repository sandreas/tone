using System.Diagnostics;
using System.Threading.Tasks;
using ATL;
using tone.Metadata;
using tone.Metadata.Formats;
using tone.Metadata.Serializers;

namespace tone.Services;
public enum SerializerFormat {
    Default,
    SpectreConsole,
    Json
}
public class SerializerService
{
    private readonly MetadataTextSerializer _text;
    private readonly SpectreConsoleSerializer _spectreConsoleSerializer;

    public SerializerService(SpectreConsoleSerializer spectreConsoleSerializer, MetadataTextSerializer text)
    {
        _spectreConsoleSerializer = spectreConsoleSerializer;
        _text = text;
    }

    public async Task<string> SerializeAsync(IMetadata metadata, SerializerFormat? format = null) => format switch
    {
        SerializerFormat.SpectreConsole => await _spectreConsoleSerializer.SerializeAsync(metadata),
        _ => await _text.SerializeAsync(metadata)
    };
}