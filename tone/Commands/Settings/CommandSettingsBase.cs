using System.ComponentModel;
using System.IO;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using tone.Commands.Settings.Interfaces;
using static System.Array;
namespace tone.Commands.Settings;

public abstract class CommandSettingsBase : CommandSettings, ILoggerSettings
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

    [CommandOption("--log-level")] public LogLevel LogLevel { get; set;  } = LogLevel.None;

    [CommandOption("--log-file")] public string LogFile { get; set;  } = Path.Combine(Path.GetTempPath() , "tone.log");

}