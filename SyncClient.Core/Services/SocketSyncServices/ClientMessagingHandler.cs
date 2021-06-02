using Microsoft.Extensions.Configuration;
using Serilog;
using SimpleTCP;
using SyncClient.Shared;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.Services.SocketSyncServices
{
    internal class ClientMessagingHandler : MessaingHandlerBase
    {
        private readonly SimpleTcpClient connector;

        public event EventHandler OnSendMessageFailed;

        public ClientMessagingHandler(IConfiguration configuration, string clientId, object extraInfo)
            : base(configuration, clientId, extraInfo)
        {
            connector = new SimpleTcpClient
            {
                Delimiter = Delimiter,
                StringEncoder = Encoding.ASCII,
            };
        }

        public override Task<bool> ConnectAsync()
        {
            if (connector.TcpClient?.Connected ?? false)
            {
                return Task.FromResult(true);
            }

            var conn = connector.Connect(Configuration.HostUrl, Configuration.Port);
            if (false == conn.TcpClient.Connected)
            {
                return Task.FromResult(false);
            }

            sendMessage(MessageTopic.Join);

            conn.DelimiterDataReceived += (sndr, se) =>
            {
                var msg = GetMessage(se.MessageString);
                switch (msg.Topic)
                {
                    case MessageTopic.Leave:
                        {
                            Log.Verbose("Server leave");
                            connector.Disconnect();
                            OnSendMessageFailed?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    default: break;
                }
            };

            Log.Verbose($"ClientId: {ClientId}");

            return Task.FromResult(true);
        }

        public override Task DisconnectAsync()
        {
            Log.Verbose("Disconnected");
            if (false == ConnectorReady())
            {
                return Task.CompletedTask;
            }

            sendMessage(MessageTopic.Leave);
            connector.Disconnect();
            return Task.CompletedTask;
        }

        public override Task SyncAsync()
        {
            Log.Verbose("Sync");
            if (false == ConnectorReady())
            {
                return Task.CompletedTask;
            }

            sendMessage(MessageTopic.Maintain);
            return Task.CompletedTask;
        }

        protected override bool ConnectorReady()
            => connector?.TcpClient?.Connected ?? false;

        private void sendMessage(MessageTopic topic)
        {
            try
            {
                var msg = CreateMessage(topic, new ClientInfo { ClientId = ClientId });
                connector.WriteLine(msg);
            }
            catch (Exception)
            {
                OnSendMessageFailed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
