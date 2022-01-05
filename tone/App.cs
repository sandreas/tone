using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using tone.Commands;
using tone.Options;

namespace tone;

public class App
{
    private readonly ICommand<TagOptions> _tag;
    private readonly ICommand<MergeOptions> _merge;
    private StringWriter _helpWriter;

    public App(ICommand<TagOptions> tag,ICommand<MergeOptions> merge,  StringWriter helpWriter)
    {
        _tag = tag;
        _merge = merge;
        _helpWriter = helpWriter;
    }
    public async Task<int> RunAsync(string[] args)
    {
        var parser = new Parser (with => with.HelpWriter = _helpWriter);
        var result= await parser.ParseArguments<TagOptions, MergeOptions>(args)
            .MapResult(
                (TagOptions options) => _tag.ExecuteAsync(options),
                (MergeOptions options) => _merge.ExecuteAsync(options),
                DisplayHelp
                 );
        return result;
    }    
    private async Task<int> DisplayHelp (IEnumerable<Error> errs)
    {
        var errsList = errs.ToImmutableArray();
        if (errsList.IsVersion () || errsList.IsHelp ()) {
            
            Console.WriteLine (_helpWriter.ToString ());
            return await Task.FromResult((int)ExitCodes.Success);
        }

        await Console.Error.WriteLineAsync (_helpWriter.ToString ());
        return await Task.FromResult((int)ExitCodes.ParseArgumentsFailed);
    }

}