using Microsoft.Extensions.Configuration;
using Serilog;
using SyncClient.Services.SocketSyncServices;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SyncClient.DemoApp
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
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();

            var sync = new SocketSyncService(configuration);
            await sync.BeginAsync(new { Name = "DemoApp", Version = ".NET Core" });

            Console.ReadLine();

            await sync.EndAsync();
        }
    }
}
