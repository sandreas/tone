using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using GrokNet;

namespace tone.Matchers;

public class AudioBookPackage
{
    public string Id { get; set; } = "";
    public AudioBookPackageType Type { get; set; } = AudioBookPackageType.Unknown;
    public IDirectoryInfo? BaseDirectory { get; set; }
    public IList<IFileInfo> Files { get; set; } = Array.Empty<IFileInfo>();
    // public Grok? Pattern { get; set; }
    public GrokResult Matches { get; set; } = new(new List<GrokItem>());
}