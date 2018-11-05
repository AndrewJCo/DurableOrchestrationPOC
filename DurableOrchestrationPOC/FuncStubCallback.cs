using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace DurableOrchestrationPOC
{
    public static class FuncStubCallback
    {
        public static readonly HttpClient HttpClient = new HttpClient();

        [FunctionName("FuncStubCallback")]
        public static async Task Run([ActivityTrigger] CallbackRequest startParameters, ILogger log)
        {
            await Task.Delay(startParameters.DelayInMilliSeconds);

            log.LogInformation("C# HTTP trigger function processed a request.");

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(startParameters.Url),
                Method = HttpMethod.Post,
                Content = new StringContent("")
            };

            await HttpClient.SendAsync(request);
        }
    }

    public class CallbackRequest
    {
        public string Url { get; set; }
        public int DelayInMilliSeconds { get; set; }
    }
}