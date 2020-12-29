using System.Diagnostics;
using System.IO;
using Ben.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace mvc
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Add blocking detector at start of Configure
            app.UseBlockingDetection();
            BlockingMonitor.BlockingMethodCalled += OnBlockingMethodCalled;
            BlockingMonitor.LogBlockingMethodCalls = false;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private void OnBlockingMethodCalled(StackTrace stackTrace, int framesToSkip)
        {
            for (var x = framesToSkip - 1; x < stackTrace.FrameCount; x++)
            {
                var frame = stackTrace.GetFrame(x);

                if (!string.IsNullOrEmpty(frame.GetFileName()))
                {
                    var fileName = Path.GetFileName(frame.GetFileName());
                    var blockingCall = string.Empty;
                    var line = frame.GetFileLineNumber();

                    if (frame.HasMethod())
                    {
                        blockingCall = frame.GetMethod().Name;
                    }

                    Debug.WriteLine($"Blocking Call {blockingCall} on {fileName}:{line}");
                    break;
                }
            }
        }
    }
}
