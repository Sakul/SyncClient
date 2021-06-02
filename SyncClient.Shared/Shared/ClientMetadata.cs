using System;

namespace SyncClient.Shared
{
    public class ClientMetadata
    {
        public string ExtraInfo { get; set; }
        public DateTime LastUpdatedTime { get; set; }

        public ClientMetadata(DateTime lastUpdatedTime, string extraInfo)
        {
            ExtraInfo = extraInfo;
            LastUpdatedTime = lastUpdatedTime;
        }
    }
}
