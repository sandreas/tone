using System;
using ATL;

namespace tone.Metadata;

public class ToneJsonFile
{
    public long Size { get; set; }
    public DateTime Created { get; set; } 
    public DateTime Modified { get; set; }
    public DateTime Accessed { get; set; }
    public string Path { get; set; }
    public string Name { get; set; }
}