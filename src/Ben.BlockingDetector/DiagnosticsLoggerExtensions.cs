using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Ben.Diagnostics
{
    internal static class DiagnosticsLoggerExtensions
    {
        // BlockingDetection
        private static readonly Action<ILogger, StackTrace, Exception> _blockingMethodCalled =
            LoggerMessage.Define<StackTrace>(LogLevel.Warning, new EventId(6, "BlockingMethodCalled"), "Blocking method has been invoked and blocked, this can lead to threadpool starvation." + Environment.NewLine + "{stackTrace}");

        public static void BlockingMethodCalled(this ILogger logger, StackTrace stackTrace)
        {
            _blockingMethodCalled(logger, stackTrace, null);
        }
    }
}
