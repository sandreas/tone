using System.Threading.Tasks;
using OperationResult;
using Sandreas.AudioMetadata;

namespace tone.Metadata.Taggers;

public interface ITagger
{
    public Task<Status<string>> UpdateAsync(IMetadata metadata);
}