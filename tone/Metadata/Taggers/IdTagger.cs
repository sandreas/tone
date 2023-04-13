using System;
using System.Threading.Tasks;
using Jint;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Commands.Settings.Interfaces;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class IdTagger: INamedTagger
{
    private readonly IIdTaggerSettings _settings;
    private readonly Engine _jint;
    private readonly AudibleIdTagger _audible;
    public string Name => nameof(IdTagger);
    public string Id { get; set; } = "";

    public IdTagger(Engine jint, AudibleIdTagger audible)
    {
        _jint = jint;
        _audible = audible;
    }
    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {
        if(string.IsNullOrWhiteSpace(Id))
        {
            return await Task.FromResult(Ok());
        }

        var id = ResolveScriptedId(metadata);
        if(!id)
        {
            return Error(id.Error);
        }
        
        return  Ok();
    }

    private Result<string, string> ResolveScriptedId(IMetadata metadata)
    {
        if(!Id.StartsWith("metadata."))
        {
            return Ok(Id);
        }
        try
        {
            _jint.SetValue("metadata", metadata);
            return Ok(_jint.Evaluate("return " + Id).AsString());
        }
        catch (Exception e)
        {
            return Error("Id could not be resolved: " + Id);
        }
    }
}