using System;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.Options;
using tone.Commands;
using tone.Options;

namespace tone;

public class App
{
    private readonly MergeCommand _merge;

    public App(MergeCommand merge)
    {
        _merge = merge;
    }
    public async Task<int> RunAsync(string[] args)
    {
        return await Parser.Default.ParseArguments<MergeOptions>(args)
            .MapResult(
                _merge.Execute,
                _ => Task.FromResult(1));
    }

}