using System.Threading.Tasks;

namespace SyncClient.Services
{
    internal interface IMessagingHandler
    {
        string FamilyId { get; }
        string ClientId { get; }

        Task SyncAsync();
        Task<bool> ConnectAsync();
        Task DisconnectAsync();
    }
}
