using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Ben.Diagnostics
{
    // Tips of the Toub
    internal sealed class DetectBlockingSynchronizationContext : SynchronizationContext
    {
        [ThreadStatic]
        private static int t_recursionCount;

        private readonly ILogger _logger;
        private readonly SynchronizationContext _syncCtx;

        public DetectBlockingSynchronizationContext(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DetectBlockingSynchronizationContext>();

            SetWaitNotificationRequired();
        }

        public DetectBlockingSynchronizationContext(ILoggerFactory loggerFactory, SynchronizationContext syncCtx) : this(loggerFactory)
        {
            _syncCtx = syncCtx;
        }

        public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            t_recursionCount++;

            try
            {
                if (t_recursionCount == 1 &&
                    Thread.CurrentThread.IsThreadPoolThread)
                {
                    _logger.BlockingMethodCalled(new StackTrace(2));
                }

                if (_syncCtx != null)
                {
                    return _syncCtx.Wait(waitHandles, waitAll, millisecondsTimeout);
                }
                else
                {
                    return base.Wait(waitHandles, waitAll, millisecondsTimeout);
                }
            }
            finally
            {
                t_recursionCount--;
            }
        }
    }
}
