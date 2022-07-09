using System;
using Spectre.Console.Cli;
using tone.Metadata;
using tone.Services;

namespace tone.Commands.Settings;

public class DumpCommandSettings : CommandSettingsBase
{
    
    [CommandOption("--include-property")]
    public MetadataProperty[] IncludeProperties { get; set; } = Array.Empty<MetadataProperty>();
    [CommandOption("--format")] public SerializerFormat Format { get; set; } = SerializerFormat.Default;
    [CommandOption("--query")] public string Query { get; set; } = "";

    /*
    public override ValidationResult Validate()
    {
        return Name.Length < 2
            ? ValidationResult.Error("Names must be at least two characters long")
            : ValidationResult.Success();
    }
    */
}