using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace SyncClient.Services.SocketSyncServices
{
    public class SocketSyncService : ISyncClientService
    {
        private string clientId;
        private object extraInfo;
        private readonly Timer syncTimer;
        private IMessagingHandler messagingHandler;

        public SocketSyncService(TimeSpan syncPeriod)
        {
            syncTimer = new Timer(syncPeriod.TotalMilliseconds);
            syncTimer.Elapsed += async (sndr, se) => await messagingHandler?.SyncAsync();
        }

        public Task<bool> BeginAsync(CancellationToken cancellationToken = default)
            => BeginAsync(null, cancellationToken);

        public Task<bool> BeginAsync<TExtraInfo>(TExtraInfo extraInfo, CancellationToken cancellationToken = default)
            where TExtraInfo : class
        {
            this.extraInfo = extraInfo;
            return BeginAsync(extraInfo, cancellationToken);
        }

        private async Task<bool> BeginAsync(object extraInfo, CancellationToken cancellationToken)
        {
            await createUniqueClientIdIfNotExist();
            clientId ??= await createSelfClientId();
            var isServiceReady = await createConnectionHandler(cancellationToken);
            if (isServiceReady)
            {
                syncTimer.Start();
            }
            return isServiceReady;

            // HACK: bypass create unique client Id
            Task createUniqueClientIdIfNotExist() => Task.CompletedTask;

            // HACK: bypass create self client Id
            Task<string> createSelfClientId() => Task.FromResult(Guid.NewGuid().ToString());

            async Task<bool> createConnectionHandler(CancellationToken cancellationToken)
            {
                do
                {
                    if (await createHost() || await createClient())
                    {
                        break;
                    }
                } while (false == cancellationToken.IsCancellationRequested);
                return null != messagingHandler;

                Task<bool> createHost() => createHandler(new HostMessaingHandler(clientId, extraInfo));
                async Task<bool> createClient()
                {
                    var client = new ClientMessagingHandler(clientId);
                    if (await createHandler(client))
                    {
                        client.OnSendMessageFailed += Client_OnSendMessageFailed;
                        return true;
                    }
                    return false;
                }
                async Task<bool> createHandler(IMessagingHandler handler)
                {
                    try
                    {
                        if (await handler.ConnectAsync())
                        {
                            messagingHandler = handler;
                        }
                        return null != messagingHandler;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
        }

        private async void Client_OnSendMessageFailed(object sender, EventArgs e)
        {
            if (sender is ClientMessagingHandler client)
            {
                syncTimer.Stop();
                Log.Verbose("Send message failed");
                await client.DisconnectAsync();
                messagingHandler = null;
                await BeginAsync(extraInfo, new CancellationTokenSource().Token);
            }
        }

        public async Task EndAsync()
        {
            syncTimer.Stop();
            await messagingHandler.DisconnectAsync();
        }

        public async void Dispose()
            => await EndAsync();
    }
}
