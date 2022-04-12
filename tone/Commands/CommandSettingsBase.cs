using System.ComponentModel;
using Spectre.Console.Cli;
using tone.Services;
using static System.Array;
namespace tone.Commands;

public abstract class CommandSettingsBase : CommandSettings
{
    [Description("Input files or folders")]
    [CommandArgument(0, "[input]")]
    public string[] Input { get; set; } = Empty<string>();


    [CommandOption("--format")] public SerializerFormat Format { get; set; } = SerializerFormat.Default;

    [CommandOption("--include-extensions")]
    public string[] IncludeExtensions { get; set; } = Empty<string>();

}