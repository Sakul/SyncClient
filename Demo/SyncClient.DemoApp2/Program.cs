using Serilog;
using SyncClient.Services.SocketSyncServices;
using System;
using System.Threading.Tasks;

namespace SyncClient.DemoApp2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            var delay = new Random().Next(1, 7);
            var sync = new SocketSyncService(TimeSpan.FromSeconds(delay));
            await sync.BeginAsync();

            Console.ReadLine();
        }
    }
}
