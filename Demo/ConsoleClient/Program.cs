using Microsoft.Extensions.Configuration;
using Serilog;
using SyncClient.Services.SocketSyncServices;
using System;
using System.Threading.Tasks;

namespace ConsoleClient
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
            await sync.BeginAsync(new { Name = "ConsoleClient", Version = ".NET Core" });

            Console.ReadLine();

            await sync.EndAsync();
        }
    }
}
