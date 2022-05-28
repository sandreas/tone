using Spectre.Console.Cli;

namespace tone.Interceptors;

public class CommandInterceptor : ICommandInterceptor
{
    private readonly CommandSettingsProvider _settingsProvider;
    public CommandInterceptor(CommandSettingsProvider settingsProvider) => _settingsProvider = settingsProvider;

    public void Intercept(CommandContext context, CommandSettings settings) => _settingsProvider.Settings = settings;
}