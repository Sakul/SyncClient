using Microsoft.Extensions.Configuration;
using Serilog;
using SyncClient.Services.SocketSyncServices;
using System;
using System.IO;
using System.Reflection;
using System.Text;
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

            var assembly = Assembly.GetEntryAssembly();
            var resourceStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.appsettings.json");
            using var reader = new StreamReader(resourceStream, Encoding.UTF8).BaseStream;
            var configuration = new ConfigurationBuilder()
                .AddJsonStream(reader)
                .Build();

            var sync = new SocketSyncService(configuration);
            await sync.BeginAsync(new { Name = "ConsoleClient", Version = ".NET Core" });

            Console.ReadLine();

            await sync.EndAsync();
        }
    }
}
