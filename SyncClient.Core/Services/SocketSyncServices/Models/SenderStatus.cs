using SyncClient.Shared;

namespace SyncClient.Services.SocketSyncServices.Models
{
    internal class SenderStatus
    {
        public MessageTopic Topic { get; set; }
        public long Timestamp { get; set; }
        public string ClientId { get; set; }
        public string FamilyId { get; set; }
        public ClientInfo ClientInfo { get; set; }
        public string ExtraInfo { get; set; }
    }
}
