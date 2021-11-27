using System.Threading.Tasks;
using tone.Options;

namespace tone.Commands;

public interface ICommand<in T>
{
    public Task<int> Execute(T options);
}