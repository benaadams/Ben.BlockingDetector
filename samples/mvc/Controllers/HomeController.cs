using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Threading.Controllers
{
    public class HomeController : Controller
    {
        private static object _lock;

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
            await Task.Run(() => BlockingTask());

            return "Hello World";
        }

        [HttpGet("/hello-async")]
        public async Task<string> HelloAsync()
        {
            // No blocking :)
            await Task.Delay(2000);

            return "Hello World";
        }

        private static void BlockingTask()
        {
            // Detected blocking
            Task.Delay(2000).Wait();
        }
    }
}
