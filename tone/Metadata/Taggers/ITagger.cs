using System.IO.Abstractions;
using System.Threading.Tasks;
using CommandLine;
using OperationResult;

namespace tone.Metadata.Taggers;

public interface ITagger
{
    public Task<Status<string>> Update(IMetadata metadata);
}