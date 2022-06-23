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
    
    public void log(params object?[] parameters)
    {
        for (var index = 0; index < parameters.Length; index++)
        {
            var param = parameters[index];
            _console.Write(param?.ToString() ?? "null");
            if(index < parameters.Length -1) {
                _console.Write(" ");
            }
        }

        _console.WriteLine();
    }
}