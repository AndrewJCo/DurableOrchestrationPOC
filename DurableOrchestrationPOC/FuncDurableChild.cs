using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrationPOC
{
    public static class FuncDurableChild
    {
        [FunctionName("FuncDurableChild")]
        public static async Task<ChildResponse> RunOrchestrator([OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            var response = new ChildResponse();
            
            var request = context.GetInput<ChildRequest>();

            using (var timeoutCts = new CancellationTokenSource())
            {
                var dueTime = context.CurrentUtcDateTime.AddMinutes(5);
                var durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);

                var eventName = "ApprovalEvent";
                if (!context.IsReplaying) log.LogInformation($"Waiting for {context.InstanceId} {eventName}");

                // Simulate an external event by kicking off a callback which will pause then trigger the event below
                string instanceId = context.InstanceId;
                var callbackRequest = new CallbackRequest
                {
                    DelayInMilliSeconds = 10000,
                    Url = "http://localhost:7071/api/FuncOrchestrationEvent?instanceid=" + instanceId + "&operationid=ApprovalEvent"
                };

#pragma warning disable 4014
                context.CallActivityAsync<CallbackRequest>("FuncStubCallback", callbackRequest);
#pragma warning restore 4014

                // Wait for the event
                var approvalEvent = context.WaitForExternalEvent<bool>(eventName);
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