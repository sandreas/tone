namespace tone.Commands.Settings.Interfaces;

public interface ICoverTaggerSettings
{
    public string[] Covers { get; }
    public bool AutoImportCovers { get; }
    
}