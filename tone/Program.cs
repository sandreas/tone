using System;
using System.Threading.Tasks;

namespace tone
{
    class Program
    {
        // private static ServiceProvider serviceProvider;

        static async Task Main(string[] args)
        {
            try
            {
                /*
                var services = new ServiceCollection();
                ConfigureServices(services);
                serviceProvider = services.BuildServiceProvider();
                Console.WriteLine("Hello World!");
                */
                await Task.Delay(10);
                Console.WriteLine("hello world");
            }
            catch (ArgumentException aex)
            {
                Console.WriteLine($"Caught ArgumentException: {aex.Message}");
            }
        }
        /*
        private static void  ConfigureServices(ServiceCollection services)
        {
            var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            var config = new ConfigurationBuilder()
                .AddJsonFile(configFile,
                    optional: true,
                    reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // https://stackoverflow.com/questions/55025197/how-to-use-configuration-with-validatedataannotations
            services.Configure<AppSettings>((options) => { config.GetSection(nameof(AppSettings)).Bind(options); });
        }
        */
    }
}