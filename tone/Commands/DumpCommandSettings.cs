using System;
using Spectre.Console.Cli;
using tone.Metadata;
using tone.Services;

namespace tone.Commands;

public class DumpCommandSettings : CommandSettingsBase
{
    
    [CommandOption("--include-property")]
    public MetadataProperty[] IncludeProperties { get; set; } = Array.Empty<MetadataProperty>();
    [CommandOption("--format")] public SerializerFormat Format { get; set; } = SerializerFormat.Default;

    /*
    public override ValidationResult Validate()
    {
        return Name.Length < 2
            ? ValidationResult.Error("Names must be at least two characters long")
            : ValidationResult.Success();
    }
    */
}