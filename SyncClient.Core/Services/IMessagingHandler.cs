using System.Threading.Tasks;

namespace SyncClient.Services
{
    internal interface IMessagingHandler
    {
        string ClientId { get; }

        Task SyncAsync();
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
    }
}
