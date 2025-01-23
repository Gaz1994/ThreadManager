namespace ThreadManager;

public class Example
{
    public Example()
    {
    }

    public ValueTask Work1()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask Work2(string somedata)
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask ExampleCode()
    {
        bool someBool = false; 
        var threadManager = new ThreadManager(); 
        
        
        /* use conditions to start a task.
         Also returns handle to store if needed. (RegisterWaitHandle) */
        var handle = threadManager.StartThreadIf(
            someBool,
            async() => await Work1(),
            "returned false"
        );
        
        // spinning up single task 
        threadManager.StartThread(async () => await Work1()); 
        
        
        
        
        
        // Spinning up on startup. 
        var handles = await threadManager
            .AddWork(async() => await Work1())
            .AddWork(async() =>  await Work2("some data"))
            .StartAllAsync(); 
    }
}