namespace ThreadManager;

public static class ThreadManagerExtensions
{
    // Existing AddWorkIf methods for group operations
    public static ThreadManager AddWorkIf(
        this ThreadManager manager,
        bool condition,
        Func<Task> action,
        string? failureMessage = null)
    {
        if (condition)
        {
            return manager.AddWork(action);
        }
        
        if (!string.IsNullOrEmpty(failureMessage))
        {
            Console.WriteLine(failureMessage);
        }
        
        return manager;
    }

    public static ThreadManager AddWorkIf(
        this ThreadManager manager,
        bool condition,
        Action action,
        string? failureMessage = null)
    {
        if (condition)
        {
            return manager.AddWork(action);
        }
        
        if (!string.IsNullOrEmpty(failureMessage))
        {
            Console.WriteLine(failureMessage);
        }
        
        return manager;
    }

    // New StartThreadIf methods for single thread operations
    public static RegisteredWaitHandle? StartThreadIf(
        this ThreadManager manager,
        bool condition,
        Func<Task> action,
        string? failureMessage = null)
    {
        if (condition)
        {
            return manager.StartThread(action);
        }
        
        if (!string.IsNullOrEmpty(failureMessage))
        {
            Console.WriteLine(failureMessage);
        }
        
        return null;
    }

    public static RegisteredWaitHandle? StartThreadIf(
        this ThreadManager manager,
        bool condition,
        Action action,
        string? failureMessage = null)
    {
        if (condition)
        {
            return manager.StartThread(action);
        }
        
        if (!string.IsNullOrEmpty(failureMessage))
        {
            Console.WriteLine(failureMessage);
        }
        
        return null;
    }
}