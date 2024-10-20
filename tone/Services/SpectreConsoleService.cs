using System;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace tone.Services;

public class SpectreConsoleService : IAnsiConsole
{
    private IAnsiConsole Output { get; }
    public IAnsiConsole Error { get; }

    public SpectreConsoleService(IAnsiConsole? stdout = null, IAnsiConsole? stderr = null)
    {
        Output = stdout ?? AnsiConsole.Console;
        Error = stderr ?? AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(Console.Error)
        });
        
        // fix unwanted wrapping if output is redirected
        if (Console.IsOutputRedirected)
        {
            Output.Profile.Width = int.MaxValue;
        }

        if (Console.IsErrorRedirected)
        {
            Error.Profile.Width = int.MaxValue;
        }
    }

    public Profile Profile => Output.Profile;
    public IAnsiConsoleCursor Cursor => Output.Cursor;
    public IAnsiConsoleInput Input => Output.Input;
    public IExclusivityMode ExclusivityMode => Output.ExclusivityMode;
    public RenderPipeline Pipeline => Output.Pipeline;

    public void Clear(bool home)
    {
        Output.Clear(home);
    }

    public void Write(IRenderable renderable)
    {
        Output.Write(renderable);
    }
}