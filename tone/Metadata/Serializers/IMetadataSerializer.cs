using System.Threading.Tasks;

namespace tone.Metadata.Serializers;

public interface IMetadataSerializer
{
    public Task<string> SerializeAsync(IMetadata metadata);
}