namespace tone.Commands.Settings.Interfaces;

public interface IScriptSettings: IIdTaggerSettings
{
    public string[] Scripts { get; }
    public string[] ScriptTaggerParameters { get; } 
}