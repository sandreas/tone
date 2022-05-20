using System.Collections.Generic;
using System.IO.Abstractions;
using GrokNet;

namespace tone.Matchers;

public class AudioBookPackage
{
    public string Id { get; set; } = "";
    public IDirectoryInfo? BaseDirectory { get; set; }
    public IList<IFileInfo> Files { get; set; } = new List<IFileInfo>();
    public GrokResult Matches { get; set; } = new(new List<GrokItem>());
}