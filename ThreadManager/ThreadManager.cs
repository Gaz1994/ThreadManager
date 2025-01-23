using System.Collections.Immutable;

namespace ThreadManager;

public sealed class ThreadManager
{
    private readonly List<(Action Action, Func<Task<bool>>? Condition)> _actions = [];
    private readonly List<(Func<Task> Action, Func<Task<bool>>? Condition)> _asyncActions = [];
    

    // Single thread starters
    public RegisteredWaitHandle StartThread(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var waitHandle = new ManualResetEventSlim(false);
        
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                action();
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in thread pool work item");
            }
            finally
            {
                waitHandle.Set();
            }
        });

        return ThreadPool.RegisterWaitForSingleObject(
            waitHandle.WaitHandle,
            (state, timedOut) =>
            {
                if (timedOut)
                {
                    Console.WriteLine("Work item timed out");
                }
                ((ManualResetEventSlim)state!).Dispose();
            },
            waitHandle,
            Timeout.InfiniteTimeSpan,
            executeOnlyOnce: true
        );
    }

    public RegisteredWaitHandle StartThread(Func<Task> asyncAction)
    {
        ArgumentNullException.ThrowIfNull(asyncAction);
        var waitHandle = new ManualResetEventSlim(false, spinCount: 1);
        
        ThreadPool.QueueUserWorkItem(async _ =>
        {
            try
            {
                await asyncAction();
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in thread pool work item");
            }
            finally
            {
                waitHandle.Set();
            }
        });

        return ThreadPool.RegisterWaitForSingleObject(
            waitHandle.WaitHandle,
            (state, timedOut) =>
            {
                if (timedOut)
                {
                    Console.WriteLine("Work item timed out");
                }
                ((ManualResetEventSlim)state!).Dispose();
            },
            waitHandle,
            Timeout.InfiniteTimeSpan,
            executeOnlyOnce: true
        );
    }

    // Group methods
    public ThreadManager AddWork(Action action, Func<Task<bool>>? condition = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        _actions.Add((action, condition));
        return this;
    }

    public ThreadManager AddWork(Func<Task> asyncAction, Func<Task<bool>>? condition = null)
    {
        ArgumentNullException.ThrowIfNull(asyncAction);
        _asyncActions.Add((asyncAction, condition));
        return this;
    }

    public async ValueTask<IReadOnlyCollection<RegisteredWaitHandle>> StartAllAsync(TimeSpan? startupTimeout = null)
    {
        var waitHandles = new List<RegisteredWaitHandle>(_actions.Count + _asyncActions.Count);

        foreach (var (action, condition) in _actions)
        {
            if (condition is not null)
            {
                if (!await condition())
                {
                    Console.WriteLine("Skipping work item due to unmet condition");
                    continue;
                }
            }

            waitHandles.Add(StartThread(action));
        }

        foreach (var (asyncAction, condition) in _asyncActions)
        {
            if (condition is not null)
            {
                if (!await condition())
                {
                    Console.WriteLine("Skipping async work item due to unmet condition");
                    continue;
                }
            }

            waitHandles.Add(StartThread(asyncAction));
        }

        return waitHandles.ToImmutableList();
    }

    // Helper method for disposing handles
    private static void DisposeHandle(RegisteredWaitHandle handle)
    {
        using var unregisterEvent = new ManualResetEvent(false);
        try
        {
            handle.Unregister(unregisterEvent);
            if (unregisterEvent.WaitOne(TimeSpan.FromSeconds(5)))
            {
                return;
            }

            // If we timeout, force cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        finally
        {
            unregisterEvent.Dispose();
        }
    }

    // Helper method for disposing multiple handles
    public static void DisposeHandles(IEnumerable<RegisteredWaitHandle> handles)
    {
        foreach (var handle in handles)
        {
            DisposeHandle(handle);
        }
    }
}