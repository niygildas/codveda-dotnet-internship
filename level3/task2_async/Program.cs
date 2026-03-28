using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║  Codveda — Async Programming & Multithreading    ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");

await Demo1_AsyncAwait();
await Demo2_TaskParallelLibrary();
await Demo3_ThreadPool();
await Demo4_RaceConditions();
await Demo5_Semaphore();
await Demo6_ProducerConsumer();

Console.WriteLine("\n✅ All demos completed!");
Console.ReadKey();

// ═══════════════════════════════════════════════════
// DEMO 1 — async/await
// ═══════════════════════════════════════════════════
static async Task Demo1_AsyncAwait()
{
    Console.WriteLine("\n╔══ DEMO 1: async/await ══════════════════════╗");
    var sw = Stopwatch.StartNew();

    var tasks = new[]
    {
        SimulateApiCall("Users API",    1200),
        SimulateApiCall("Products API", 800),
        SimulateApiCall("Orders API",   1500)
    };

    var results = await Task.WhenAll(tasks);
    sw.Stop();

    foreach (var r in results)
        Console.WriteLine($"  ✅ {r}");

    Console.WriteLine($"  Total time: {sw.ElapsedMilliseconds}ms (vs ~3500ms sequential)");
}

static async Task<string> SimulateApiCall(string name, int delayMs)
{
    await Task.Delay(delayMs);
    return $"{name} responded in {delayMs}ms";
}

// ═══════════════════════════════════════════════════
// DEMO 2 — Task Parallel Library
// ═══════════════════════════════════════════════════
static async Task Demo2_TaskParallelLibrary()
{
    Console.WriteLine("\n╔══ DEMO 2: Task Parallel Library ════════════╗");

    var items = Enumerable.Range(1, 10).ToList();
    var results = new ConcurrentBag<string>();
    var sw = Stopwatch.StartNew();

    await Task.Run(() =>
        Parallel.ForEach(items,
            new ParallelOptions { MaxDegreeOfParallelism = 4 },
            item =>
            {
                Thread.Sleep(100);
                results.Add($"Item {item} processed on Thread {Thread.CurrentThread.ManagedThreadId}");
            })
    );

    sw.Stop();
    Console.WriteLine($"  Processed {results.Count} items in {sw.ElapsedMilliseconds}ms");

    var sum = await Task.Run(() =>
        Enumerable.Range(1, 1_000_000)
                  .AsParallel()
                  .Where(n => n % 2 == 0)
                  .Select(n => (long)n)
                  .Sum());

    Console.WriteLine($"  PLINQ sum of even 1-1M: {sum:N0}");
}

// ═══════════════════════════════════════════════════
// DEMO 3 — ThreadPool & CancellationToken
// ═══════════════════════════════════════════════════
static async Task Demo3_ThreadPool()
{
    Console.WriteLine("\n╔══ DEMO 3: ThreadPool & Cancellation ════════╗");

    using var cts = new CancellationTokenSource(3000);
    var token = cts.Token;

    Console.WriteLine("  Starting work with 3 second timeout...");

    try
    {
        await Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                token.ThrowIfCancellationRequested();
                Console.WriteLine($"  Working... step {i + 1}/10");
                await Task.Delay(400, token);
            }
        }, token);

        Console.WriteLine("  ✅ Work completed!");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("  ⚠ Work cancelled after timeout!");
    }

    var countdown = new CountdownEvent(3);
    for (int i = 1; i <= 3; i++)
    {
        int taskNum = i;
        ThreadPool.QueueUserWorkItem(_ =>
        {
            Thread.Sleep(200 * taskNum);
            Console.WriteLine($"  ThreadPool item {taskNum} done");
            countdown.Signal();
        });
    }
    countdown.Wait();
}

// ═══════════════════════════════════════════════════
// DEMO 4 — Race Conditions
// ═══════════════════════════════════════════════════
static async Task Demo4_RaceConditions()
{
    Console.WriteLine("\n╔══ DEMO 4: Race Conditions & Thread Safety ══╗");

    int unsafeCounter = 0;
    int safeCounter = 0;
    int atomicCounter = 0;
    var lockObj = new object();
    int iterations = 10000;

    // unsafe
    var t1 = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
    {
        for (int i = 0; i < iterations / 10; i++) unsafeCounter++;
    }));
    await Task.WhenAll(t1);
    Console.WriteLine($"  ❌ Unsafe counter: {unsafeCounter} (expected {iterations})");

    // with lock
    var t2 = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
    {
        for (int i = 0; i < iterations / 10; i++)
            lock (lockObj) { safeCounter++; }
    }));
    await Task.WhenAll(t2);
    Console.WriteLine($"  ✅ Safe counter (lock): {safeCounter}");

    // interlocked
    var t3 = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
    {
        for (int i = 0; i < iterations / 10; i++)
            Interlocked.Increment(ref atomicCounter);
    }));
    await Task.WhenAll(t3);
    Console.WriteLine($"  ✅ Atomic counter (Interlocked): {atomicCounter}");
}

// ═══════════════════════════════════════════════════
// DEMO 5 — SemaphoreSlim
// ═══════════════════════════════════════════════════
static async Task Demo5_Semaphore()
{
    Console.WriteLine("\n╔══ DEMO 5: SemaphoreSlim — Rate Limiting ════╗");
    Console.WriteLine("  10 requests, max 3 concurrent");

    var semaphore = new SemaphoreSlim(3, 3);

    var tasks = Enumerable.Range(1, 10).Select(async i =>
    {
        Console.WriteLine($"  Request {i:D2} waiting...");
        await semaphore.WaitAsync();
        try
        {
            Console.WriteLine($"  Request {i:D2} ▶ processing");
            await Task.Delay(500);
            Console.WriteLine($"  Request {i:D2} ✅ done");
        }
        finally
        {
            semaphore.Release();
        }
    });

    await Task.WhenAll(tasks);
}

// ═══════════════════════════════════════════════════
// DEMO 6 — Producer Consumer
// ═══════════════════════════════════════════════════
static async Task Demo6_ProducerConsumer()
{
    Console.WriteLine("\n╔══ DEMO 6: Producer-Consumer ════════════════╗");

    var queue = new BlockingCollection<string>(boundedCapacity: 5);

    var producer = Task.Run(async () =>
    {
        for (int i = 1; i <= 8; i++)
        {
            var item = $"Order-{i:D3}";
            queue.Add(item);
            Console.WriteLine($"  📦 Produced: {item}");
            await Task.Delay(100);
        }
        queue.CompleteAdding();
    });

    var consumer = Task.Run(async () =>
    {
        foreach (var item in queue.GetConsumingEnumerable())
        {
            Console.WriteLine($"  ✅ Consumed: {item}");
            await Task.Delay(250);
        }
    });

    await Task.WhenAll(producer, consumer);
    Console.WriteLine("  Producer-Consumer complete!");
}