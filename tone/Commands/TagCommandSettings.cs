using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ATL;
using Spectre.Console.Cli;
using tone.Metadata;

namespace tone.Commands;

public class TagCommandSettings : TagSettingsBase
{

    [CommandOption("--assume-yes|-y")] public bool AssumeYes { get; init; } = false;
    [CommandOption("--dry-run")] public bool DryRun { get; init; } = false;


    // todo: https://github.com/spectresystems/spectre.cli/issues/92
    // todo: meta-cover
    /*
    public override ValidationResult Validate()
    {
        return Name.Length < 2
            ? ValidationResult.Error("Names must be at least two characters long")
            : ValidationResult.Success();
    }
    */
}
