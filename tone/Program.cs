using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using tone.Commands;
using tone.Common.Io;
using tone.Options;
using tone.Services;
using static System.Environment.SpecialFolder;
using static System.Environment.SpecialFolderOption;

namespace tone;

class Program
{

    static async Task<int> Main(string[] args)
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
            configBuilder.AddJsonFile(f,true,true);

        }
        var config = configBuilder.Build();
        
        var services = new ServiceCollection();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();
        
        // only for testing
        // Log.Error("error");
        
        // https://stackoverflow.com/questions/55025197/how-to-use-configuration-with-validatedataannotations
        services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));
        services.AddTransient<StringWriter>();
        
        
        services.AddSingleton<FileSystem>();
        services.AddSingleton<FileWalker>();

        services.AddSingleton<App>();
        services.AddSingleton<ICommand<TagOptions>, TagCommand>();
        services.AddSingleton<ICommand<MergeOptions>, MergeCommand>();
        
        services.AddSingleton<TagService>();
        services.AddSingleton<JobBuilderService>();
        services.AddSingleton<DirectoryLoaderService>();
        services.AddLogging(builder => builder.AddSerilog(dispose:true));
        
        var runAppResultCode = await services.BuildServiceProvider().GetRequiredService<App>().RunAsync(args);
        return await Task.FromResult(runAppResultCode);

        //https://www.thecodebuzz.com/dependency-injection-console-app-using-generic-hostbuilder/
        // https://devblogs.microsoft.com/ifdef-windows/command-line-parser-on-net5/
        // https://stackoverflow.com/questions/54912012/how-to-stop-exit-terminate-dotnet-core-hostbuilder-console-application-programma
        // libs
        // https://github.com/commandlineparser/commandline
        /*
        try
        {
            // return Environment.ExitCode;
            
            await Host.CreateDefaultBuilder().ConfigureServices(services =>
            {
                services.AddSingleton<ITagService, TagService>();
            }).UseConsoleLifetime(opts => opts.SuppressStatusMessages = true).RunConsoleAsync();
            
        }
        catch (Exception aex)
        {
            Console.WriteLine($"Caught ArgumentException: {aex.Message}");
             
        }
        return Environment.ExitCode;
        */

    }



    //}

}