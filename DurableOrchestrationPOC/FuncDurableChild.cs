using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrationPOC
{
    public static class FuncDurableChild
    {
        [FunctionName("FuncDurableChild")]
        public static async Task<ChildResponse> RunOrchestrator([OrchestrationTrigger] DurableOrchestrationContext ctx,
            ILogger log)
        {
            var response = new ChildResponse();

            var request = ctx.GetInput<ChildRequest>();

            using (var timeoutCts = new CancellationTokenSource())
            {
                var dueTime = ctx.CurrentUtcDateTime.AddMinutes(5);
                var durableTimeout = ctx.CreateTimer(dueTime, timeoutCts.Token);

                var eventName = "ApprovalEvent" + request.City;
                log.LogInformation($"Waiting for {ctx.InstanceId} {eventName}");


                var approvalEvent = ctx.WaitForExternalEvent<bool>(eventName);
                if (approvalEvent == await Task.WhenAny(approvalEvent, durableTimeout))
                {
                    timeoutCts.Cancel();

                    response.City = request.City;
                }
            }

            return response;
        }
    }
}