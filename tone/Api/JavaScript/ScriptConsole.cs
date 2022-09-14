using System;
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

    private void LogHelper(Func<string, Markup> callWrapper, params object?[] parameters)
    {
        for (var index = 0; index < parameters.Length; index++)
        {
            var param = parameters[index];
            _console.Write(callWrapper(param?.ToString() ?? "null"));
            if (index < parameters.Length - 1)
            {
                _console.Write(" ");
            }
        }

        _console.WriteLine();
    }

    // ReSharper disable once InconsistentNaming
    public void clear()
    {
        _console.Clear();
    }
    // ReSharper disable once InconsistentNaming

    public void log(params object?[] parameters)
    {
        LogHelper(s => new Markup(Markup.Escape(s)), parameters);
    }
    // ReSharper disable once InconsistentNaming

    public void debug(params object?[] parameters)
    {
        LogHelper(s => new Markup("[silver]:right_arrow: " + Markup.Escape(s) + "[/]"), parameters);
        // 
    }
    // ReSharper disable once InconsistentNaming

    public void info(params object?[] parameters)
    {
        // :information:
        LogHelper(s => new Markup("[blue]:information: " + Markup.Escape(s) + "[/]"), parameters);
    }
    // ReSharper disable once InconsistentNaming

    public void warn(params object?[] parameters)
    {
        LogHelper(s => new Markup("[yellow]:warning: " + Markup.Escape(s) + "[/]"), parameters);
    }
    // ReSharper disable once InconsistentNaming

    public void error(params object?[] parameters)
    {
        LogHelper(s => new Markup("[red]:red_exclamation_mark: " + Markup.Escape(s) + "[/]"), parameters);
    }
}