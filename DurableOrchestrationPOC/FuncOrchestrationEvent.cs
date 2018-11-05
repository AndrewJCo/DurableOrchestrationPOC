using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrationPOC
{
    public static class FuncOrchestrationEvent
    {
        [FunctionName("FuncOrchestrationEvent")]
        public static async Task<IActionResult> OrchestrationEvent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string instanceId = req.Query["instanceid"];
            string operationId = req.Query["operationId"];

            await client.RaiseEventAsync(instanceId, operationId, true);

            return new OkObjectResult("");
        }
    }
}