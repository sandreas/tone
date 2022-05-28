using System.ComponentModel;
using Spectre.Console.Cli;
using static System.Array;
namespace tone.Commands.Settings;

public abstract class CommandSettingsBase : CommandSettings
{
    [CommandOption("--debug")] public bool Debug { get; set; } = false;
    [Description("Input files or folders")]
    [CommandArgument(0, "[input]")]
    public string[] Input { get; set; } = Empty<string>();

//    [Description("Use taggers in given order")]
//    [CommandOption("--tagger")]
//    public string[] Taggers { get; set; } = Empty<string>();


    [CommandOption("--include-extension")]
    public string[] IncludeExtensions { get; set; } = Empty<string>();

    
}