# Ben.BlockingDetector
Blocking Detection for ASP.NET Core

[![NuGet version (Ben.Demystifier)](https://img.shields.io/nuget/v/Ben.BlockingDetector.svg?style=flat-square)](https://www.nuget.org/packages/Ben.BlockingDetector/)
[![Build status](https://ci.appveyor.com/api/projects/status/7xssvgr4coj898cq?svg=true)](https://ci.appveyor.com/project/benaadams/ben-blockingdetector)

## Detect Blocking Calls in ASP.NET Core Applications

Blocking calls can lead to ThreadPool starvation. Ouputs a warning to the log when blocking calls are made on the ThreadPool.

### Usage

Add early to your ASP.NET Core pipeline

```csharp
app.UseBlockingDetection();
```

### Caveats

Doesn't detect everything... so its not a panacea; you should actively try to avoid using blocking calls.

1. For `async` methods with occurances of **`.ConfigureAwait(false)`**, detection won't alert for blocking calls after occurances in the case where the returned `Task` wasn't already completed
2. Won't alert for blocking calls that don't block, like on precompleted `Task`s (e.g. a single small `Body.Write`)
3. Won't alert for blocking that happens in syscalls (e.g. `File.Read(...)`, `Thread.Sleep`)

Will detect CLR initiated waits `lock`,`ManualResetEventSlim`,`Semaphore{Slim}`,`Task.Wait`,`Task.Result` etc; if they do block.

### Example
If you had a method like:

```csharp
public Task BlockingWrite(HttpContext httpContext)
{
    var response = httpContext.Response;
    response.StatusCode = 200;
    response.ContentType = "text/plain";

    var s = new string('n', 160000);
    response.ContentLength = s.Length * 3;
    response.Body.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
    response.Body.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);
    response.Body.Write(Encoding.ASCII.GetBytes(s), 0, s.Length);

    return Task.CompletedTask;
}
```

It would output to your log

```csharp
warn: Ben.Diagnostics.DetectBlocking[6]
  Blocking method has been invoked and blocked, this can lead to threadpool starvation.
    at System.Threading.ManualResetEventSlim.Wait(Int32 millisecondsTimeout, CancellationToken cancellationToken)
    at System.Threading.Tasks.Task.SpinThenBlockingWait(Int32 millisecondsTimeout, CancellationToken cancellationToken)
    at System.Threading.Tasks.Task.InternalWait(Int32 millisecondsTimeout, CancellationToken cancellationToken)
    at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
    at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.FrameResponseStream.Write(Byte[] buffer, Int32 offset, Int32 count)
    at AoA.Gaia.Startup.BlockingWrite(HttpContext httpContext)
    at AoA.Gaia.SpaceWebSocketsMiddleware.Invoke(HttpContext httpContext)
    at Microsoft.AspNetCore.WebSockets.WebSocketMiddleware.Invoke(HttpContext context)
    at AoA.Space.ErrorHandlerMiddleware.<Invoke>d__4.MoveNext()
    at System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start[TStateMachine](TStateMachine& stateMachine)
    at AoA.Space.ErrorHandlerMiddleware.Invoke(HttpContext httpContext)
    at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware.<Invoke>d__7.MoveNext()
    at System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start[TStateMachine](TStateMachine& stateMachine)
    at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware.Invoke(HttpContext context)
    at AoA.Gaia.BlockingDetection.BlockingDetectionMiddleware.<Invoke>d__3.MoveNext()
    at System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start[TStateMachine](TStateMachine& stateMachine)
    at AoA.Gaia.BlockingDetection.BlockingDetectionMiddleware.Invoke(HttpContext httpContext)
    at Microsoft.AspNetCore.Hosting.Internal.RequestServicesContainerMiddleware.<Invoke>d__3.MoveNext()
    at System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start[TStateMachine](TStateMachine& stateMachine)
    at Microsoft.AspNetCore.Hosting.Internal.RequestServicesContainerMiddleware.Invoke(HttpContext httpContext)
    at Microsoft.AspNetCore.Hosting.Internal.HostingApplication.ProcessRequestAsync(Context context)
    at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.Frame`1.<ProcessRequestsAsync>d__2.MoveNext()
    at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
    at Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.Pipe.<>c.<.cctor>b__67_3(Object o)
    at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure.LoggingThreadPool.<>c__DisplayClass6_0.<Schedule>b__0()
    at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure.LoggingThreadPool.<RunAction>b__3_0(Object o)
    at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
    at System.Threading.ThreadPoolWorkQueue.Dispatch()
```

Also catches held locks

```csharp
120: public Task LockedMethod(HttpContext httpContext)
121: {
122:     var response = httpContext.Response;
123:     response.StatusCode = 200;
124:     response.ContentType = "text/plain";
125:     lock (obj) // **********
126:     {
127:         // locked outside for 1 sec
128:     }
129:     var s = new string('n', 16);
130:     response.ContentLength = s.Length;
131:     return response.WriteAsync(s);
132: }
```
Outputs (with some extra formatting)
```csharp
warn: Microsoft.AspNetCore.Diagnostics.DetectBlocking[6]
 Blocking method has been invoked and blocked, this can lead to threadpool starvation.
     at Task AoA.Gaia.Startup.LockedMethod(HttpContext httpContext)
       in C:\Work\AoA\src\AoA.Gaia\Startup.cs:line 125 ********
     at IApplicationBuilder Microsoft.AspNetCore.Builder.UseExtensions.Use(IApplicationBuilder app, Func<HttpContext, Func<Task>, Task> middleware)+() => { }
....
     at void Microsoft.AspNetCore.Server.Kestrel.Internal.System.IO.Pipelines.Pipe._scheduleContinuation(object o)
     at void Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure.LoggingThreadPool.Schedule(Action<object> action, object state)+() => { }
     at void Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure.LoggingThreadPool.RunAction()+(object o) => { }
     at bool System.Threading.ThreadPoolWorkQueue.Dispatch()
```
