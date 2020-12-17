using System;
using System.Diagnostics.Tracing;
using System.Threading;

namespace Ben.Diagnostics
{
    // Tips of the Toub
    internal sealed class TaskBlockingListener : EventListener
    {
        private static readonly Guid s_tplGuid = new Guid("2e5dba47-a3d2-4d16-8ee0-6671ffdcd7b5");

        [ThreadStatic]
        private static int t_recursionCount;

        private readonly BlockingMonitor _monitor;

        public TaskBlockingListener(BlockingMonitor monitor)
        {
            _monitor = monitor;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Guid == s_tplGuid)
            {
                // 3 == Task|TaskTransfer
                EnableEvents(eventSource, EventLevel.Verbose, (EventKeywords) 3);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (!Thread.CurrentThread.IsThreadPoolThread)
            {
                return;
            }

            if (eventData.EventId == 10 && // TASKWAITBEGIN_ID
                eventData.Payload != null &&
                eventData.Payload.Count > 3 &&
                eventData.Payload[3] is int value && // Behavior
                value == 1) // TaskWaitBehavior.Synchronous
            {
                t_recursionCount++;
                _monitor.BlockingStart(DectectionSource.EventListener);
            }
            else if (eventData.EventId == 11 // TASKWAITEND_ID
                     && t_recursionCount > 0)
            {
                t_recursionCount--;
                _monitor.BlockingEnd();
            }
        }
    }
}
