using System.Collections.Generic;
using CommandLine;

namespace tone.Options;

[Verb("tag", HelpText = "tag audio files")]
public class TagOptions: OptionsBase
{
    [Option('i', "input", HelpText = "Input file(s) to process")]
    public IEnumerable<string> Input { get; set; } = new List<string>();
    
    [Option("include-extensions", Separator = ',', Required = false, HelpText = "Set output to verbose messages.")]
    public IEnumerable<string> IncludeExtensions { get; set; } = new List<string>();

    [Option("album", HelpText = "album tag")]
    public string? Album { get; set; } = null;

    [Option("artist", HelpText = "artist tag")]
    public string? Artist { get; set; } = null;
}