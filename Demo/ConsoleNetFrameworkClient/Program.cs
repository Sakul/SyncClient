using Microsoft.Extensions.Configuration;
using Serilog;
using SyncClient.Services.SocketSyncServices;
using System;
using System.Threading.Tasks;

namespace ConsoleNetFrameworkClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .Build();

            var sync = new SocketSyncService(configuration);
            await sync.BeginAsync(new { Name = "ConsoleNetFrameworkClient", Version = ".NET Framework" });

            Console.ReadLine();

            await sync.EndAsync();
        }
    }
}
