using Spectre.Console;
using tone.Services;

namespace tone.Api.JavaScript;

public class ScriptConsole
{
    private readonly SpectreConsoleService _console;

    public ScriptConsole(SpectreConsoleService console)
    {
        _console = console;
    }
    
    public void log(params object[] parameters)    {
        foreach(var param in parameters){
            _console.WriteLine(param?.ToString() ?? "null");
        }
    }
}