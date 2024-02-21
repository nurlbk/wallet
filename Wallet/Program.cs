using System.Reflection;

namespace Wallet {
    public class Program {
        public static void Main(string[] args) {
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, configuration) => {
                    configuration
                        .AddJsonFile("appsettings.localsettings.json", optional: true)
                        .AddUserSecrets(Assembly.GetEntryAssembly(), true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }

}