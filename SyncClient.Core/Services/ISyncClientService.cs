using System.Threading;
using System.Threading.Tasks;

namespace SyncClient.Services
{
    public interface ISyncClientService
    {
        Task<bool> BeginAsync(CancellationToken cancellationToken = default);
        Task<bool> BeginAsync<TExtraInfo>(TExtraInfo extraInfo, CancellationToken cancellationToken = default) where TExtraInfo : class;
        Task EndAsync();
    }
}
