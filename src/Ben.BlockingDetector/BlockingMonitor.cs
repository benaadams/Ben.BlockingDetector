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
        /// 
        /// StackTrace - The Stack Trace of the current callstack including blocking monitor itself
        /// int - The number of stack frames to skip over in order to exclude blocking monitor
        /// </summary>
        public static event Action<StackTrace, int> BlockingMethodCalled;

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
                    var framesToSkip = dectectionSource == DectectionSource.SynchronizationContext ? 3 : 6;

                    if (BlockingMonitor.LogBlockingMethodCalls)
                    {
                        _logger.BlockingMethodCalled(new StackTrace(framesToSkip, true));
                    }

                    // We omit the frame creation here becuase including a skip and file details doesn't return
                    // file numbers in .NET core 3.1
                    BlockingMonitor.BlockingMethodCalled?.Invoke(new StackTrace(true), framesToSkip);
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
