using System.Threading.Tasks;
using tone.Metadata;

namespace tone.Serializers;

public interface IMetadataSerializer
{
    public Task<string> SerializeAsync(IMetadata metadata);
}