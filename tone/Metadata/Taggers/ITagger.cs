using System.Threading.Tasks;
using OperationResult;

namespace tone.Metadata.Taggers;

public interface ITagger
{
    public Task<Status<string>> UpdateAsync(IMetadata metadata);
}