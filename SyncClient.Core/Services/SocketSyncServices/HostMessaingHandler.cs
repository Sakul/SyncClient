using Microsoft.Extensions.Logging;
using Serilog;
using SimpleTCP;
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
        private readonly object extraInfo;
        private readonly SimpleTcpServer connector;

        private DateTime currentTime => DateTime.UtcNow;

        public HostMessaingHandler(string clientId, object extraInfo)
            : base(clientId)
        {
            this.extraInfo = extraInfo;
            connector = new SimpleTcpServer
            {
                Delimiter = Delimiter,
                StringEncoder = Encoding.ASCII,
            };
        }

        public override Task<bool> ConnectAsync()
        {
            if (connector?.IsStarted ?? false)
            {
                return Task.FromResult(true);
            }

            var conn = connector.Start(DefaultPort);
            if (false == conn.IsStarted)
            {
                return Task.FromResult(false);
            }

            clientInfo = new ClientInfo
            {
                ClientId = ClientId,
                Clients = new Dictionary<string, DateTime> { { ClientId, currentTime } }
            };

            conn.DelimiterDataReceived += (sender, se) =>
            {
                var msg = GetMessage(se.MessageString);
                switch (msg.Topic)
                {
                    case MessageTopic.Join:
                        {
                            Log.Verbose($"Client join: {msg.ClientId}");
                            clientInfo.Clients.Remove(msg.ClientId);
                            clientInfo.Clients.Add(msg.ClientId, currentTime);
                            break;
                        }
                    case MessageTopic.Leave:
                        {
                            Log.Verbose($"Client leave: {msg.ClientId}");
                            clientInfo.Clients.Remove(msg.ClientId);
                            break;
                        }
                    case MessageTopic.Maintain:
                        {
                            Log.Verbose($"Client maintain: {msg.ClientId}");
                            clientInfo.Clients.Remove(msg.ClientId);
                            clientInfo.Clients.Add(msg.ClientId, currentTime);
                            break;
                        }
                    default: break;
                }
            };

            // TODO: Connect server
            Log.Verbose("Connect to the server");

            return Task.FromResult(true);
        }

        public override Task DisconnectAsync()
        {
            if (false == ConnectorReady())
            {
                return Task.CompletedTask;
            }

            var msg = CreateMessage(MessageTopic.Leave, clientInfo);
            connector.BroadcastLine(msg);
            connector.Stop();
            return Task.CompletedTask;
        }

        public override Task SyncAsync()
        {
            if (false == ConnectorReady())
            {
                return Task.CompletedTask;
            }

            var inactiveClientIds = clientInfo.Clients.Where(it => it.Value.AddMinutes(5) <= currentTime).ToList();
            foreach (var item in inactiveClientIds)
            {
                clientInfo.Clients.Remove(item.Key);
            }

            // TODO: Sync to server
            Log.Verbose($"Server sync (clients: {clientInfo.Clients.Count})");
            return Task.CompletedTask;
        }

        protected override bool ConnectorReady()
            => connector?.IsStarted ?? false;
    }
}
