using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using tone.Commands;
using tone.Services;

namespace tone;

class Program
{

    static async Task<int> Main(string[] args)
    {
        
        var services = new ServiceCollection();
        var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        var config = new ConfigurationBuilder()
            .AddJsonFile(configFile,true,true)
            .AddEnvironmentVariables()
            .Build();

        // https://stackoverflow.com/questions/55025197/how-to-use-configuration-with-validatedataannotations
        services.Configure<AppSettings>(options => config.GetSection(nameof(AppSettings)).Bind(options) );
        
        services.AddSingleton<App>();
        services.AddSingleton<MergeCommand>();
        
        services.AddSingleton<TagService>();
        
        // var serviceProvider = services.BuildServiceProvider();
        // var app = serviceProvider.GetRequiredService<App>();
        return await services.BuildServiceProvider().GetRequiredService<App>().RunAsync(args);

        
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