using System;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Options;
using tone.Commands;
using tone.Options;

namespace tone;

public class App
{
    private const int CodeInvalidCommand = 1;
    private readonly ICommand<MergeOptions> _merge;

    public App(ICommand<MergeOptions> merge)
    {
        _merge = merge;
    }
    public async Task<int> RunAsync(string[] args)
    {
        var result = await Parser.Default.ParseArguments<MergeOptions>(args)
            .MapResult(
                _merge.Execute,
                _ => Task.FromResult(CodeInvalidCommand));
        return result;
    }

}