using System.Threading.Tasks;
using tone.Metadata;
using tone.Metadata.Serializers;

namespace tone.Services;
public enum SerializerFormat {
    Default,
    Ffmetadata
}
public class SerializerService
{
    private readonly FfmetadataSerializer _ffmetaSerializer;
    private readonly SpectreConsoleSerializer _spectreConsoleSerializer;

    public SerializerService(SpectreConsoleSerializer spectreConsoleSerializer, FfmetadataSerializer ffmetaSerializer)
    {
        _spectreConsoleSerializer = spectreConsoleSerializer;
        _ffmetaSerializer = ffmetaSerializer;
    }

    public async Task<string> SerializeAsync(IMetadata metadata, SerializerFormat? format = null) => format switch
    {
        SerializerFormat.Ffmetadata => await _ffmetaSerializer.SerializeAsync(metadata),
        _ => await _spectreConsoleSerializer.SerializeAsync(metadata)
    };
}