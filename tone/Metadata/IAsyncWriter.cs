using System.Threading.Tasks;

namespace tone.Metadata;

public interface IAsyncWriter<in TInput, in TOutput, TResult>
{
    public Task<TResult> WriteAsync(TInput input, TOutput output);
}