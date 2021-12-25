using System;
using System.Threading.Tasks;
using tone.Options;
using tone.Services;

namespace tone.Commands;

public class MergeCommand: ICommand<MergeOptions>
{
    private readonly TagService _tag;

    public MergeCommand(TagService tag)
    {
        _tag = tag;
    }
    public async Task<int> ExecuteAsync(MergeOptions options)
    {
        Console.WriteLine("Merge");
        _tag.DoSomething();
        return await Task.FromResult(5);
    }

}