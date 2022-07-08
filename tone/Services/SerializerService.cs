using System.Threading.Tasks;
using tone.Metadata;
using tone.Metadata.Serializers;

namespace tone.Services;
public enum SerializerFormat {
    Default,
    Ffmetadata,
    JsonMetadata
}
public class SerializerService
{
    private readonly FfmetadataSerializer _ffmetaSerializer;
    private readonly SpectreConsoleSerializer _spectreConsoleSerializer;
    private readonly JsonMetadataSerializer _jsonMetaSerializer;

    public SerializerService(SpectreConsoleSerializer spectreConsoleSerializer, FfmetadataSerializer ffmetaSerializer, JsonMetadataSerializer jsonMetaSerializer)
    {
        _spectreConsoleSerializer = spectreConsoleSerializer;
        _ffmetaSerializer = ffmetaSerializer;
        _jsonMetaSerializer = jsonMetaSerializer;
    }

    public async Task<string> SerializeAsync(IMetadata metadata, SerializerFormat? format = null) => format switch
    {
        SerializerFormat.Ffmetadata => await _ffmetaSerializer.SerializeAsync(metadata),
        SerializerFormat.JsonMetadata => await _jsonMetaSerializer.SerializeAsync(metadata),
        _ => await _spectreConsoleSerializer.SerializeAsync(metadata)
    };
}