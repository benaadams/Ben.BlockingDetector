using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Ben.Diagnostics
{
    class ExampleBlockingMethodCallback
    {
        public void SetupCallback()
        {
            BlockingMonitor.BlockingMethodCalled += OnBlockingMethodCalled;
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
