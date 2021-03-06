using System.ComponentModel;
using Spectre.Console.Cli;
using static System.Array;
namespace tone.Commands.Settings;

public abstract class CommandSettingsBase : CommandSettings
{
    [CommandOption("--debug")] public bool Debug { get; set; } = false;
    [CommandOption("--force")] public bool Force { get; set; } = false;

    [Description("Input files or folders")]
    [CommandArgument(0, "[input]")]
    public string[] Input { get; set; } = Empty<string>();
    
    [CommandOption("--include-extension")]
    public string[] IncludeExtensions { get; set; } = Empty<string>();

    [CommandOption("--order-by")] public string OrderBy { get; set; } = "";

    [CommandOption("--limit")] public string Limit { get; set; } = "";

    
}