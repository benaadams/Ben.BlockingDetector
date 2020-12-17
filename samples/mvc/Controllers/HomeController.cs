using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

#pragma warning disable IDE0007 
namespace mvc
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public Task Slower()
        {
            // Detected blocking in called method
            return DoSomethingAsync();
        }

        private async Task DoSomethingAsync()
        {
            await Task.Delay(1000);

            // Detected blocking
            Task.Run (() => { Thread.Sleep(1000); }).Wait();
        }

        [HttpGet("/hello")]
        public string Hello()
        {
            // Not detected: Thread.Sleep happens at OS level
            Thread.Sleep(2000);

            return "Hello World";
        }

        [HttpGet("/hello-sync-over-async")]
        public string HelloSyncOverAsync()
        {
            // Detected blocking
            Task.Delay(2000).Wait();

            return "Hello World";
        }

        [HttpGet("/hello-async-over-sync")]
        public async Task<string> HelloAsyncOverSync()
        {
            // Detected blocking in called method
            var result = await Task.Run(() => BlockingTask());

            return $"Hello World {result}";
        }

        private static int BlockingTask()
        {
            // Detected blocking
            return MethodAsync().Result;
        }

        private static async Task<int> MethodAsync()
        {
            await Task.Delay(1000);
            return 5;
        }

        [HttpGet("/hello-async")]
        public async Task<string> HelloAsync()
        {
            // No blocking :)
            await Task.Delay(2000);

            return "Hello World";
        }

        [HttpGet("/hello-async-precompleted")]
        public async Task<string> HelloAsyncPrecompleted()
        {
            Task<int> task = MethodAsync();

            if (!task.IsCompletedSuccessfully)
            {
                // Await completes the Task without blocking
                await task;
            }
            
            // The task is completed at this point so .Result doesn't trigger blocking
            var result = task.Result;

            return $"Hello World {result}";
        }
    }

#pragma warning restore IDE0007 
}
