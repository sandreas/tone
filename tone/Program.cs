using System;
using System.IO;
using System.IO.Abstractions;
using CliFx;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sandreas.Files;
using Serilog;
using tone.Commands;
using tone.Metadata.Formats;
using tone.Metadata.Serializers;
using tone.Services;
using static System.Environment.SpecialFolder;
using static System.Environment.SpecialFolderOption;


// var stream = new MemoryStream(Encoding.Default.GetBytes("NOTDIRSEP [^/\\\\]*"));
// var grok = new Grok("input/%{NOTDIRSEP:genre}/", stream);
// var result = grok.Parse("input/Fantasy/");



var configBuilder = new ConfigurationBuilder().AddEnvironmentVariables();

var configFiles = new[]
{
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"),
    Path.Combine(Environment.GetFolderPath(UserProfile, DoNotVerify), ".tone/appsettings.json"),
    Path.Combine(Environment.GetFolderPath(ApplicationData, DoNotVerify), "tone/appsettings.json"),
};
// configBuilder.AddJsonFile(configFiles.First(), true, true);
foreach (var f in configFiles)
{
    // beware: Setting "reloadOnChange" to true for non existant files will slow down 
    // the app a lot
    configBuilder.AddJsonFile(f, true, false);
}

var config = configBuilder.AddEnvironmentVariables().Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

foreach (var f in configFiles)
{
    Log.Logger.Debug("trying to load config file {File}", f);    
}

var services = new ServiceCollection();
// services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));
services.AddTransient<StringWriter>();
services.AddSingleton<FileSystem>();
services.AddSingleton<FileWalker>();
services.AddSingleton<DirectoryLoaderService>();
services.AddSingleton<GrokPatternService>();
services.AddSingleton<ChptFmtNativeMetadataFormat>();
services.AddSingleton<MetadataTextSerializer>();
services.AddSingleton<SerializerService>();

services.AddSingleton<TagCommand>();
services.AddSingleton<DumpCommand>();
// services.AddSingleton<TagService>();
// services.AddSingleton<JobBuilderService>();
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
    Console.WriteLine(aex.StackTrace);
    Environment.ExitCode = 1;
}

return Environment.ExitCode;