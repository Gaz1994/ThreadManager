# ThreadManager
 
Just some code examples to use this library: 

```cs
        var threadManager = new ThreadManager(); 
        
        
        /* use conditions to start a task.
         Also returns handle to store if needed. (RegisterWaitHandle) */
        var handle = threadManager.StartThreadIf(
            someBool,
            async() => await Work1(),
            "returned false"
        );
        
        // spinning up single task (also returns a handle if needed). 
        threadManager.StartThread(async () => await Work1()); 
        
        
        
        
        
        // Spinning up on startup. 
        var handles = await threadManager
            .AddWork(async() => await Work1())
            .AddWork(async() =>  await Work2("some data"))
            .StartAllAsync(); 
```

Refer to ```Example.cs``` for more info. 

Easily helps you spin up tasks and can spin them down, you can store the handles and dipose them using the ```DisposeHandle(handle)``` method within ThreadManager. 


