# SyncClient

## Setup
1.Install nuget package
```
Microsoft.Extensions.Configuration.Json
```

2.Create new `appsettings.json` file
```
"AppSync": {
	"Local": 6,
	"Server": 5,
	"SyncApiFqdn": "localhost:44371"
},
"SocketSync": {
	"Port": 5020,
	"HostUrl": "127.0.0.1"
}
```

3.Setup the app configuration
```csharp
var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", true)
	.Build();
```

4.Start sync service
```csharp
// Socket service
var sync = new SocketSyncService(configuration);
await sync.BeginAsync();
```
> Optional: You can add an extra information, this data will send to your server tool
```csharp
var extraInfo = new { AppName = "App02", Version = 2.1 };
await sync.BeginAsync(extraInfo);
```

5.Stop sync service
```csharp
await sync.EndAsync();
```

## Main interface
```csharp
public interface ISyncClientService
{
	Task<bool> BeginAsync(CancellationToken cancellationToken = default);
	Task<bool> BeginAsync<TExtraInfo>(TExtraInfo extraInfo, CancellationToken cancellationToken = default) where TExtraInfo : class;
	Task EndAsync();
}
```