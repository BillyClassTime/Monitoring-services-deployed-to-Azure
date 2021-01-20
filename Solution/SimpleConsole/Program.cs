using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;
using Polly;
namespace SimpleConsole
{
    public class Program
    {
        private const string _api = "https://smpapibmvb2021.azurewebsites.net/";
        private static HttpClient _client = new HttpClient(new PollyHandler()) { BaseAddress = new Uri(_api) };
        static void Main(string[] args)
        {
            Run().Wait();
        }
        static async Task Run()
        {
            int loops = 1;
            do
            {
                string response = await _client.GetStringAsync("/weatherforecast");
                WriteLine(response);
            } while (loops++ < 1000);
        }
    }

    public class PollyHandler : DelegatingHandler
    {
        public PollyHandler() : base(new HttpClientHandler()) {}
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>  
            Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult<HttpResponseMessage>( x=> !x.IsSuccessStatusCode)
            .WaitAndRetryForeverAsync(
                retryAttempt => TimeSpan.FromSeconds(5),
                (ex,time) => WriteLine("Failed Attempt")
            )
            .ExecuteAsync(()=> base.SendAsync(request,cancellationToken));          
    }

}
