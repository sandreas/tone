namespace tone.Commands.Settings.Interfaces;

public interface IToneJsonTaggerSettings
{
    public string[] ToneJsonFiles { get; }
    public bool AutoImportToneJson { get; }
    
}