using System.Threading.Tasks;
using OperationResult;

namespace tone.Common.Data;

public interface IAsyncWriter<in TInput, in TOutput, TResult>
{
    public Task<TResult> WriteAsync(TInput input, TOutput output);
}