using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tone.Common.Extensions.Stream;
using tone.Metadata.Formats;

namespace tone.Metadata.Serializers;

public class JsonMetadataSerializer : IMetadataSerializer
{
    private readonly JsonMetadata _jsonMeta;
    private readonly JsonSerializerSettings _settings;

    public JsonMetadataSerializer(JsonMetadata jsonMeta, JsonSerializerSettings settings)
    {
        _jsonMeta = jsonMeta;
        _settings = settings;
    }

    public async Task<string> SerializeAsync(IMetadata metadata)
    {
        _jsonMeta.OverwriteProperties(metadata, metadata.MappedAdditionalFields.Keys);
        var result = JsonConvert.SerializeObject(_jsonMeta, _settings);
        return await Task.FromResult(result);
    }
}