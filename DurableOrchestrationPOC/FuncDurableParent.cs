using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrationPOC
{
    public static class FuncDurableParent
    {
        public static int BatchLimit = 1;

        // https://docs.microsoft.com/en-us/azure/azure-functions/durable-functions-sub-orchestrations

        [FunctionName("FuncDurableParent")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            var tasks = new List<Task<ChildResponse>>
            {
                context.CallSubOrchestratorAsync<ChildResponse>("FuncDurableChild", new ChildRequest {City = "Tokyo"}),
                context.CallSubOrchestratorAsync<ChildResponse>("FuncDurableChild", new ChildRequest {City = "Seattle"}),
                context.CallSubOrchestratorAsync<ChildResponse>("FuncDurableChild", new ChildRequest {City = "London"})
            };

            var sampleTask = new Task<ChildResponse>()

            //var tasks = new List<Task<ChildResponse>>
            //{
            //    context.CallSubOrchestratorAsync<ChildResponse>("FuncDurableChild", new ChildRequest {City = "Tokyo"})
            //    //context.CallSubOrchestratorAsync<ChildResponse>("FuncDurableChild", new ChildRequest {City = "Seattle"}),
            //    //context.CallSubOrchestratorAsync<ChildResponse>("FuncDurableChild", new ChildRequest {City = "London"})
            //};

            //await Task.WhenAll(tasks);

            foreach(var taskBatch in tasks.Batch(BatchLimit))
            {
                await Task.WhenAll(taskBatch);

                foreach (var finishedTask in taskBatch) outputs.Add(finishedTask.Result.City);
            }

            return outputs;
        }

        

        [FunctionName("FuncDurableParent_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
            HttpRequestMessage req,
            [OrchestrationClient] DurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await starter.StartNewAsync("FuncDurableParent", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> items, int maxItems)
        {
            return items.Select((item, inx) => new { item, inx })
                .GroupBy(x => x.inx / maxItems)
                .Select(g => g.Select(x => x.item));
        }
    }
}