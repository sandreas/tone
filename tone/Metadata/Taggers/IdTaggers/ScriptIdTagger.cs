using System.Collections.Generic;
using Jint;

namespace tone.Metadata.Taggers.IdTaggers;

public class ScriptIdTagger: ScriptTagger, IIdTagger
{
    public ScriptIdTagger(Engine engine, string name, IEnumerable<string> parameters) : base(engine, name, parameters)
    {
    }

    public string Id { get; set; } = "";
    public bool SupportsId(string id) => true;
}