using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CliFx;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sandreas.Files;
using Serilog;
using tone.Commands;
using tone.Options;
using tone.Services;
using static System.Environment.SpecialFolder;
using static System.Environment.SpecialFolderOption;

namespace tone;

static class Program
{
    public static async Task<int> Main()
    {

        var configFiles = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"),
            Path.Combine(Environment.GetFolderPath(UserProfile, DoNotVerify), "/.tone/appsettings.json"),
            Path.Combine(Environment.GetFolderPath(ApplicationData, DoNotVerify), "/tone/appsettings.json"),
        };

        var configBuilder = new ConfigurationBuilder().AddEnvironmentVariables();
        foreach (var f in configFiles)
        {
            configBuilder.AddJsonFile(f, true, true);
        }

        var config = configBuilder.Build();


        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();

        var services = new ServiceCollection();

        services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));
        services.AddTransient<StringWriter>();
        services.AddSingleton<FileSystem>();
        services.AddSingleton<FileWalker>();
        services.AddSingleton<TagCommand>();
        // services.AddSingleton<TagService>();
        // services.AddSingleton<JobBuilderService>();
        services.AddSingleton<DirectoryLoaderService>();
        services.AddLogging(builder => builder.AddSerilog(dispose: true));
        services.AddSingleton<ILogger>(_ => Log.Logger);

        var serviceProvider = services.BuildServiceProvider();
        try
        {
            return await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(serviceProvider.GetService)
                .Build()
                .RunAsync();
        }
        catch (Exception aex)
        {
            Console.WriteLine($"Uncaught ArgumentException: {aex.Message}");
            Environment.ExitCode = 1;
        }

        return Environment.ExitCode;
    }
}