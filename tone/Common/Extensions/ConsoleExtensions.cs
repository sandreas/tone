using CliFx.Infrastructure;
using Serilog;

namespace tone.Common.Extensions;

public static class ConsoleExtensions
{
    public async static void WriteInfoLine(this IConsole console, string message)
    {
        Log.Logger.Information(message);
        // console.ForegroundColor = ConsoleColor.Red;
        // await console.Error.WriteLineAsync(message);
    }
    public async static void WriteErrorLine(this IConsole console, string message)
    {
        Log.Logger.Error(message);
        // console.ForegroundColor = ConsoleColor.Red;
        // await console.Error.WriteLineAsync(message);
    }
}