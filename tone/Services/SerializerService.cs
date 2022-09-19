using System.Threading.Tasks;
using Sandreas.AudioMetadata;
using tone.Serializers;

namespace tone.Services;
public enum SerializerFormat {
    Default,
    Ffmetadata,
    Json,
    ChptFmtNative
}
public class SerializerService
{
    private readonly FfmetadataSerializer _ffmetaSerializer;
    private readonly SpectreConsoleSerializer _spectreConsoleSerializer;
    private readonly ToneJsonSerializer _toneJsonMetaSerializer;
    private readonly ChptFmtNativeSerializer _chptFmtNativeSerializer;

    public SerializerService(
        SpectreConsoleSerializer spectreConsoleSerializer, 
        FfmetadataSerializer ffmetaSerializer, 
        ToneJsonSerializer toneJsonMetaSerializer,
        ChptFmtNativeSerializer chptFmtNativeSerializer
        )
    {
        _spectreConsoleSerializer = spectreConsoleSerializer;
        _ffmetaSerializer = ffmetaSerializer;
        _toneJsonMetaSerializer = toneJsonMetaSerializer;
        _chptFmtNativeSerializer = chptFmtNativeSerializer;
    }

    public async Task<string> SerializeAsync(IMetadata metadata, SerializerFormat? format = null) => format switch
    {
        SerializerFormat.Ffmetadata => await _ffmetaSerializer.SerializeAsync(metadata),
        SerializerFormat.Json => await _toneJsonMetaSerializer.SerializeAsync(metadata),
        SerializerFormat.ChptFmtNative => await _chptFmtNativeSerializer.SerializeAsync(metadata),
        _ => await _spectreConsoleSerializer.SerializeAsync(metadata)
    };
}