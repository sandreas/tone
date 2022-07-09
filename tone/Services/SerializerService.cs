using System.Threading.Tasks;
using tone.Metadata;
using tone.Metadata.Serializers;

namespace tone.Services;
public enum SerializerFormat {
    Default,
    Ffmetadata,
    Json
}
public class SerializerService
{
    private readonly FfmetadataSerializer _ffmetaSerializer;
    private readonly SpectreConsoleSerializer _spectreConsoleSerializer;
    private readonly ToneJsonSerializer _toneJsonMetaSerializer;

    public SerializerService(SpectreConsoleSerializer spectreConsoleSerializer, FfmetadataSerializer ffmetaSerializer, ToneJsonSerializer toneJsonMetaSerializer)
    {
        _spectreConsoleSerializer = spectreConsoleSerializer;
        _ffmetaSerializer = ffmetaSerializer;
        _toneJsonMetaSerializer = toneJsonMetaSerializer;
    }

    public async Task<string> SerializeAsync(IMetadata metadata, SerializerFormat? format = null) => format switch
    {
        SerializerFormat.Ffmetadata => await _ffmetaSerializer.SerializeAsync(metadata),
        SerializerFormat.Json => await _toneJsonMetaSerializer.SerializeAsync(metadata),
        _ => await _spectreConsoleSerializer.SerializeAsync(metadata)
    };
}