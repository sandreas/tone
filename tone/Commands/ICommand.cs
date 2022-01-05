using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using tone.Options;

namespace tone.Commands;

public interface ICommand<in T>
{
    public Task<int> ExecuteAsync(T options);
}