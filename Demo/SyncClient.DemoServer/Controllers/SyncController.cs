using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SyncClient.Shared;
using System.Collections.Generic;
using System.Linq;

namespace SyncClient.DemoServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SyncController
    {
        private readonly ILogger<SyncController> logger;
        private static Dictionary<string, ClientInfo> clients;

        public SyncController(ILogger<SyncController> logger)
        {
            this.logger = logger;
            clients ??= new Dictionary<string, ClientInfo>();
        }

        [HttpGet]
        public Dictionary<string, ClientInfo> Get()
            => clients;

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        public void Sync([FromBody] ClientInfo req)
        {
            var isRequestValid = false == string.IsNullOrWhiteSpace(req?.ClientId)
                && false == string.IsNullOrWhiteSpace(req?.FamilyId)
                && (req?.Clients?.Any() ?? false);
            if (false == isRequestValid)
            {
                return;
            }

            logger.LogInformation(nameof(Sync), req);
            if (false == clients.TryAdd(req.FamilyId, req))
            {
                clients[req.FamilyId] = req;
            }
        }
    }
}
