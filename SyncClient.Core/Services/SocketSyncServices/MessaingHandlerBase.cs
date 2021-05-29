using Microsoft.Extensions.Logging;
using SyncClient.Services.SocketSyncServices.Models;
using SyncClient.Shared;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SyncClient.Services.SocketSyncServices
{
    internal abstract class MessaingHandlerBase : IMessagingHandler
    {
        private const char DelimiterCharacter = '#';
        protected const byte Delimiter = (byte)DelimiterCharacter;
        protected const int DefaultPort = 5003;

        public string ClientId { get; }

        public MessaingHandlerBase(string clientId)
            => ClientId = clientId;

        protected string CreateMessage(MessageTopic topic, ClientInfo clientInfo)
        {
            var data = new SenderStatus
            {
                Topic = topic,
                ClientId = ClientId,
                ClientInfo = clientInfo,
                Timestamp = DateTime.UtcNow.Ticks,
            };
            return JsonSerializer.Serialize(data);
        }

        protected SenderStatus GetMessage(string content)
            => JsonSerializer.Deserialize<SenderStatus>(content.Replace(DelimiterCharacter, ' '));

        public abstract Task SyncAsync();
        public abstract Task<bool> ConnectAsync();
        public abstract Task DisconnectAsync();
        protected abstract bool ConnectorReady();
    }
}
