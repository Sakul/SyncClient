using System;
using System.Collections.Generic;

namespace SyncClient.Shared
{
    public class ClientInfo
    {
        public string ClientId { get; set; }
        public Dictionary<string, DateTime> Clients { get; set; }
    }
}
