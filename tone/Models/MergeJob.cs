using System.Collections.Generic;
using tone.Options;

namespace tone.Models;

public class MergeJob
{
    public IEnumerable<string> InputFiles { get; set; }
    public string OutputFile { get; set; }
    public string BaseDir { get; set; }
    public MergeOptions Options { get; set; }


}