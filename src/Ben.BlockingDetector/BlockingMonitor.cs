using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Ben.Diagnostics
{
    public class BlockingMonitor
    {
        [ThreadStatic]
        private static int t_recursionCount;

        private readonly ILogger _logger;

        /// <summary>
        /// Gets or sets if the monitor should log blocking method calls
        /// </summary>
        public static bool LogBlockingMethodCalls { get; set; } = true;

        /// <summary>
        /// Event that is dispatched when a blocking method is detected
        /// </summary>
        public static event Action<StackTrace> BlockingMethodCalled;

        public BlockingMonitor(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BlockingMonitor>();
        }


        public void BlockingStart(DectectionSource dectectionSource)
        {
            if (!Thread.CurrentThread.IsThreadPoolThread)
            {
                return;
            }

            t_recursionCount++;

            try
            {
                if (t_recursionCount == 1)
                {
                    var stackTrace = new StackTrace(dectectionSource == DectectionSource.SynchronizationContext ? 3 : 6);

                    if (BlockingMonitor.LogBlockingMethodCalls)
                    {
                        _logger.BlockingMethodCalled(stackTrace);
                    }
                    
                    BlockingMonitor.BlockingMethodCalled?.Invoke(stackTrace);
                }
            }
            catch
            {
            }
        }

        public void BlockingEnd()
        {
            if (!Thread.CurrentThread.IsThreadPoolThread)
            {
                return;
            }

            t_recursionCount--;
        }
    }

    public enum DectectionSource
    {
        SynchronizationContext,
        EventListener
    }
}
