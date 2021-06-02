using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SyncClient.Shared;

namespace SyncClient.DemoServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SyncController
    {
        private readonly ILogger<SyncController> logger;
        private static ClientInfo clientInfo;

        public SyncController(ILogger<SyncController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public ClientInfo Get()
            => clientInfo;

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        public void Sync([FromBody] ClientInfo req)
        {
            logger.LogInformation(nameof(Sync), req);
            clientInfo = req;
        }
    }
}
