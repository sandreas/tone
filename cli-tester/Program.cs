
using cli_tester;
using cli_tester.Commands;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;


var services = new ServiceCollection();


var app = new CommandApp(new TypeRegistrar(services));

app.Configure(config =>
{
    config.UseStrictParsing();
    config.CaseSensitivity(CaseSensitivity.None);
    config.SetApplicationName("cli-tester");
    config.SetApplicationVersion("0.0.1");
    config.ValidateExamples();
    config.AddCommand<TestCommand>("test")
        .WithDescription("test spectre.console")
        .WithExample(new[] { "test", "--help" })
        .WithExample(new[] { "test", "--order-by", "", "input.mp3" })
        // .WithExample(new[] { "test", "--dry-run", "input.mp3" })
       ;
    
        config.PropagateExceptions();

    config.ValidateExamples();
});
try
{
    return await app.RunAsync(args).ConfigureAwait(false);
}
catch (Exception e)
{
    if (e is CommandParseException { Pretty: { } } ce)
    {
        AnsiConsole.Write(ce.Pretty);
    }
    AnsiConsole.WriteException(e);
    return 1;
}