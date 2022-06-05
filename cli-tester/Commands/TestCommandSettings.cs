using System.ComponentModel;
using Spectre.Console.Cli;

namespace cli_tester.Commands;

public class TestCommandSettings: CommandSettings
{
    [CommandOption("--dry-run")] public bool? DryRun { get; init; } = false;
    
    [CommandOption("--order-by")] public string? OrderBy { get; set; }
    
    [Description("Input files or folders")]
    [CommandArgument(0, "[input]")]
    public string[] Input { get; set; } = Array.Empty<string>();
}