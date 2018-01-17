using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ben.Diagnostics
{
    public class BlockingDetectionMiddleware
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly RequestDelegate _next;

        private readonly BlockingMonitor _monitor;
        private readonly DetectBlockingSynchronizationContext _detectBlockingSyncCtx;
        private readonly TaskBlockingListener _listener;

        public BlockingDetectionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IApplicationLifetime lifetime)
        {
            _next = next;
            _loggerFactory = loggerFactory;
            // Detect blocking
            _monitor = new BlockingMonitor(loggerFactory);
            _detectBlockingSyncCtx = new DetectBlockingSynchronizationContext(_monitor);
            _listener = new TaskBlockingListener(_monitor);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var syncCtx = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(syncCtx == null ? _detectBlockingSyncCtx : new DetectBlockingSynchronizationContext(_monitor, syncCtx));
            
            try
            {
                await _next(httpContext);
            }

            finally
            {
                SynchronizationContext.SetSynchronizationContext(syncCtx);
            }
        }
    }

    public static class BlockingDetectionMiddlewareExtensions
    {
        public static IApplicationBuilder UseBlockingDetection(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BlockingDetectionMiddleware>();
        }
    }
}
