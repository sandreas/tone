using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jint;
using OperationResult;
using Sandreas.AudioMetadata;
using tone.Metadata.Taggers.IdTaggers.Audible;
using static OperationResult.Helpers;

namespace tone.Metadata.Taggers;

public class IdTaggerComposite : INamedTagger
{
    // private readonly IIdTaggerSettings _settings;
    private readonly Engine _jint;
    private readonly List<IIdTagger> _idTaggers = new();
    public string Name => nameof(IdTaggerComposite);
    public string Id { get; set; } = "";

    public IdTaggerComposite(Engine jint, AudibleIdTagger? audible=null)
    {
        _jint = jint;
        if (audible != null)
        {
            _idTaggers.Add(audible);
        }
    }

    public void AddIdTagger(IIdTagger tagger)
    {
        _idTaggers.Add(tagger);
    }
    
    public async Task<Status<string>> UpdateAsync(IMetadata metadata, IMetadata? originalMetadata = null)
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            return await Task.FromResult(Ok());
        }

        var idResult = ResolveScriptedId(metadata);
        if (!idResult)
        {
            return Error(idResult.Error);
        }

        Id = idResult.Value;

        foreach (var tagger in _idTaggers)
        {
            if (tagger.SupportsId(Id))
            {
                tagger.Id = Id;
                _ = await tagger.UpdateAsync(metadata);
            }
        }

        return Ok();
    }

    private Result<string, string> ResolveScriptedId(IMetadata metadata)
    {
        if (!Id.StartsWith("metadata."))
        {
            return Ok(Id);
        }

        try
        {
            _jint.SetValue("metadata", metadata);
            return Ok(_jint.Evaluate("return " + Id).AsString());
        }
        catch (Exception)
        {
            return Error("Id could not be resolved: " + Id);
        }
    }
}