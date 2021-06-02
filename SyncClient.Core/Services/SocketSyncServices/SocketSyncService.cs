using Microsoft.Extensions.Configuration;
using Serilog;
using SyncClient.Models;
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
        private readonly AppSyncOptions appSync;
        private readonly IConfiguration configuration;
        private IMessagingHandler messagingHandler;

        public SocketSyncService(IConfiguration configuration)
        {
            this.configuration = configuration;
            const string AppSync = nameof(AppSync);
            syncTimer = new Timer();
            syncTimer.Elapsed += async (sndr, se) => await messagingHandler?.SyncAsync();
            appSync = configuration.GetSection(AppSync).Get<AppSyncOptions>();
            if (null == appSync)
            {
                throw new ArgumentNullException("Configuration's AppSync is missing");
            }
        }

        public Task<bool> BeginAsync(CancellationToken cancellationToken = default)
            => beginAsync(null, cancellationToken);

        public Task<bool> BeginAsync<TExtraInfo>(TExtraInfo extraInfo, CancellationToken cancellationToken = default)
            where TExtraInfo : class
        {
            this.extraInfo = extraInfo;
            return beginAsync(extraInfo, cancellationToken);
        }

        private async Task<bool> beginAsync(object extraInfo, CancellationToken cancellationToken)
        {
            await createFamilyIdIfNotExist();
            clientId ??= await createSelfClientId();
            var isServiceReady = await createConnectionHandler(cancellationToken);
            if (isServiceReady)
            {
                syncTimer.Start();
            }
            return isServiceReady;

            // HACK: bypass create family Id
            Task createFamilyIdIfNotExist() => Task.CompletedTask;

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

                async Task<bool> createHost()
                {
                    var host = new HostMessaingHandler(configuration, clientId, extraInfo);
                    if (await createHandler(host))
                    {
                        syncTimer.Interval = TimeSpan.FromSeconds(appSync.Server).TotalMilliseconds;
                        return true;
                    }
                    return false;
                }
                async Task<bool> createClient()
                {
                    var client = new ClientMessagingHandler(configuration, clientId, extraInfo);
                    if (await createHandler(client))
                    {
                        syncTimer.Interval = TimeSpan.FromSeconds(appSync.Local).TotalMilliseconds;
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
                await beginAsync(extraInfo, new CancellationTokenSource().Token);
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
