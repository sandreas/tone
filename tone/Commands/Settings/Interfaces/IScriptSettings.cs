namespace tone.Commands.Settings.Interfaces;

public interface IScriptSettings
{
    public string[] Scripts { get; }
    public string[] ScriptTaggerParameters { get; } 
}