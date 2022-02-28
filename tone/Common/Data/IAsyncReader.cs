using System.Threading.Tasks;
using OperationResult;

namespace tone.Common.Data;

public interface IAsyncReader<in TSource, TResult>
{
    public Task<TResult> ReadAsync(TSource input);
}