using System.Collections.Generic;

namespace SyncClient.Shared
{
    public class ClientInfo
    {
        public string ClientId { get; set; }
        public string FamilyId { get; set; }
        public Dictionary<string, ClientMetadata> Clients { get; set; }
    }
}
