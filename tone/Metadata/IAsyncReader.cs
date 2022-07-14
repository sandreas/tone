using System.Threading.Tasks;

namespace tone.Metadata;

public interface IAsyncReader<in TSource, TResult>
{
    public Task<TResult> ReadAsync(TSource input);
}