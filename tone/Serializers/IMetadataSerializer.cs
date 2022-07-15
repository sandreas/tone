using System.Threading.Tasks;
using Sandreas.AudioMetadata;

namespace tone.Serializers;

public interface IMetadataSerializer
{
    public Task<string> SerializeAsync(IMetadata metadata);
}