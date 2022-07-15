using System.Collections.Generic;
using System.Threading.Tasks;
using Jint;
using OperationResult;
using Sandreas.AudioMetadata;
using static OperationResult.Helpers;
namespace tone.Metadata.Taggers;

public class ScriptTagger: INamedTagger
{
    private readonly IEnumerable<string> _parameters;
    public string Name { get; set; }
    private Engine Engine { get; set; }

    // https://blog.codeinside.eu/2019/06/30/jint-invoke-javascript-from-dotnet/
    public ScriptTagger(Engine engine, string name, IEnumerable<string> parameters)
    {
        Engine = engine;
        Name = name;
        _parameters = parameters;
    }

    public async Task<Status<string>> UpdateAsync(IMetadata metadata)
    {
        Engine.Invoke(Name, metadata, _parameters);
        return await Task.FromResult(Ok());
    }

}