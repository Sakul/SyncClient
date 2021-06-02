using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using SimpleTCP;
using SyncClient.Models;
using SyncClient.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.Services.SocketSyncServices
{
    internal class HostMessaingHandler : MessaingHandlerBase
    {
        private ClientInfo clientInfo;
        private string syncApiUrl;
        private readonly SimpleTcpServer connector;

        private DateTime currentTime => DateTime.UtcNow;

        public HostMessaingHandler(IConfiguration configuration, string familyId, string clientId, object extraInfo)
            : base(configuration, familyId, clientId, extraInfo)
        {
            connector = new SimpleTcpServer
            {
                Delimiter = Delimiter,
                StringEncoder = Encoding.ASCII,
            };
            const string AppSync = nameof(AppSync);
            var apiFqdn = configuration
                .GetSection(AppSync)
                .Get<AppSyncOptions>()
                ?.SyncApiFqdn ?? throw new ArgumentNullException("AppSync's SyncApiFqdn is missing");
            var baseUrl = $"https://{apiFqdn}";
            syncApiUrl = $"{baseUrl}/sync";
        }

        public override async Task<bool> ConnectAsync()
        {
            if (connector?.IsStarted ?? false)
            {
                return true;
            }

            var conn = connector.Start(Configuration.Port);
            if (false == conn.IsStarted)
            {
                return false;
            }

            clientInfo = new ClientInfo
            {
                ClientId = ClientId,
                FamilyId = FamilyId,
                Clients = new Dictionary<string, ClientMetadata>
                {
                    { ClientId, new ClientMetadata(currentTime, ExtraInfoJson) }
                }
            };

            conn.DelimiterDataReceived += async (sender, se) =>
            {
                var msg = GetMessage(se.MessageString);
                switch (msg.Topic)
                {
                    case MessageTopic.Join:
                        {
                            Log.Verbose($"Client join: {msg.ClientId}");
                            clientInfo.Clients.Remove(msg.ClientId);
                            clientInfo.Clients.Add(msg.ClientId, new ClientMetadata(currentTime, msg.ExtraInfo));
                            await SyncAsync();
                            break;
                        }
                    case MessageTopic.Leave:
                        {
                            Log.Verbose($"Client leave: {msg.ClientId}");
                            clientInfo.Clients.Remove(msg.ClientId);
                            await SyncAsync();
                            break;
                        }
                    case MessageTopic.Maintain:
                        {
                            Log.Verbose($"Client maintain: {msg.ClientId}");
                            clientInfo.Clients.Remove(msg.ClientId);
                            clientInfo.Clients.Add(msg.ClientId, new ClientMetadata(currentTime, msg.ExtraInfo));
                            break;
                        }
                    default: break;
                }
            };

            Log.Verbose("SERVER");
            Log.Verbose($"FamilyId: {FamilyId}, ClientId: {clientInfo.ClientId}");
            Log.Verbose($"ExtraInfo: {ExtraInfoJson}");
            await syncApiUrl.PostJsonAsync(clientInfo);
            return true;
        }

        public override Task DisconnectAsync()
        {
            Log.Verbose("Disconnected");
            if (false == ConnectorReady())
            {
                return Task.CompletedTask;
            }

            var msg = CreateMessage(MessageTopic.Leave, clientInfo);
            connector.BroadcastLine(msg);
            connector.Stop();
            return Task.CompletedTask;
        }

        public override async Task SyncAsync()
        {
            if (false == ConnectorReady())
            {
                return;
            }

            var inactiveClientIds = clientInfo.Clients.Where(it => it.Value.LastUpdatedTime.AddMinutes(2) <= currentTime).ToList();
            foreach (var item in inactiveClientIds)
            {
                clientInfo.Clients.Remove(item.Key);
            }

            Log.Verbose($"Server sync (clients: {clientInfo.Clients.Count})");
            await syncApiUrl.PostJsonAsync(clientInfo);
        }

        protected override bool ConnectorReady()
            => connector?.IsStarted ?? false;
    }
}
