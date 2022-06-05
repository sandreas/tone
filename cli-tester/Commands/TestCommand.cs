using Spectre.Console.Cli;

namespace cli_tester.Commands;

public class TestCommand: AsyncCommand<TestCommandSettings>
{

    public override Task<int> ExecuteAsync(CommandContext context, TestCommandSettings settings)
    {
        Console.WriteLine($"order-by=<{settings.OrderBy ?? "<null>"}>");
        return Task.FromResult(0);
    }
}