using System;
using System.Collections.Generic;
using Jint;
using tone.Metadata.Taggers;

namespace tone.Services;

public class JavaScriptApi
{
    private readonly Engine _jint;
    private readonly TaggerComposite _tagger;
    private readonly IEnumerable<string> _customTaggerParameters = Array.Empty<string>();

    public JavaScriptApi()
    {
        // fallback "dummy" constructor in case of missing prerequisites
        _jint = new Engine();
        _tagger = new TaggerComposite();
    }
    public JavaScriptApi(Engine jint, TaggerComposite tagger, IEnumerable<string> customTaggerParameters)
    {
        _jint = jint;
        _tagger = tagger;
        _customTaggerParameters = customTaggerParameters;
    }

    public void RegisterTagger(string name)
    {
        _tagger.Taggers.Add(new ScriptTagger(_jint, name, _customTaggerParameters));
    }
}