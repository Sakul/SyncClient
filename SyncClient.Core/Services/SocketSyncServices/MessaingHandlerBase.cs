using Microsoft.Extensions.Configuration;
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
        protected const string SocketSync = nameof(SocketSync);
        protected readonly SocketSyncOptions Configuration;

        public string ClientId { get; }
        public MessaingHandlerBase(IConfiguration configuration, string clientId)
        {
            ClientId = clientId;
            const string SocketSync = nameof(SocketSync);
            Configuration = configuration
                .GetSection(SocketSync)
                .Get<SocketSyncOptions>();
            Configuration ??= new SocketSyncOptions
            {
                Port = 5003,
                HostUrl = "127.0.0.1",
            };
        }

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
