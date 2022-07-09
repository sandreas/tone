using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using tone.Common.Extensions.Stream;
using tone.Metadata.Formats;

namespace tone.Metadata.Serializers;

public class ToneJsonSerializer : IMetadataSerializer
{
    private readonly ToneJson _toneJson;
    private readonly JsonSerializerSettings _settings;

    public ToneJsonSerializer(ToneJson toneJson, JsonSerializerSettings settings)
    {
        _toneJson = toneJson;
        _settings = settings;
    }

    public async Task<string> SerializeAsync(IMetadata metadata)
    {
        _toneJson.OverwriteProperties(metadata, metadata.MappedAdditionalFields.Keys);

        var container = new ToneJsonContainer()
        {
            Meta = _toneJson,

        };
        
        var result = JsonConvert.SerializeObject(container, _settings);
        return await Task.FromResult(result);
    }
}