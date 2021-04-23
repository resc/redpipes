RedPipes
========

A .NET toolkit for building execution pipelines.

Inspired by [GreenPipes](https://github.com/phatboyg/GreenPipes) of [MassTransit](http://masstransit-project.com/) fame, with a dash of [Go contexts](https://golang.org/pkg/context/)

Example
-------

```csharp
using RedPipes;
using RedPipes.Auth;

public class Program 
{

    private static IPipe<Request> _requestHandler;

    public static void Main()
    {
        // create the pipe for denied requests
        var accessDenied = Pipe.Build<Request>()
            .UseAsync(AccessDenied);

        // create request pipeline
        _requestHandler = Pipe.Build<Request>()
            .UsePrincipalProvider(BearerTokenClaimsPrincipalProvider)
            .UseAuthPolicy(p => p.HasRole("admin").And(p.IsAfter(startDate)), accessDenied)
            .UseAsync(HandleRequest)
            .Build();

        // start your transport here, to feed the pipe with requests
        ...
    }
    
    // process the request to extract/look up ClaimsPrincipal
    private async Task<ClaimsPrincipal> BearerTokenClaimsPrincipalProvider(IContext ctx, Request req)
    {
        var bearerToken = ExtractBearerTokenFromRequest(req);
        var principal = await CreateNewClaimsPrincipal(bearerToken);
        return principal;
    }
    
    // handle the request
    private static  async Task HandleRequest(IContext ctx, Request req)
    {
        var principal = ctx.GetPrincipal();
        var greeting = req.Greeting;

        // perform work
        await Console.Out.WriteLineAsync($"{principal.Identity.Name} says: {greeting}");
    }
    
    // handle denied requests
    private static async Task AccessDenied(IContext ctx, Request value, Pipe.ExecuteAsync<Request> next)
    {
        var p = ctx.GetPrincipal();
        
        await Console.Out.WriteLineAsync($"Access denied for user '{p.Identity.Name}'");
        await next(ctx, value);
    }
}
```

# Features

## Pipe Primitives
---------------
These are implemented as several extension methods

#### Execute extensions
Add an simple execute delegate to the pipeline.
Executes the delegate, and the next section in the pipeline unconditionally
```csharp
builder
    .Use((ctx, value) => {
        Console.WriteLine(value);
    })
    . Use(...); // Executed unconditionally...
```

#### Delegated extensions
Add a full pipe delegate to the pipeline. 
Execute this delegate and the pipe delegate decides wether or not to execute the next section of the pipeline

```csharp
builder
    .UseAsync(async (ctx, value, next) => {
        await Console.WriteLineAsync("pre-execute");
        
        await next(ctx, value);

        await Console.WriteLineAsync("post-execute");
    })
    . Use(...); // Executed only when next(ctx, value) is called...
```

#### Branch extensions
Add an alernate branch to the pipeline, and execute that when the condition is true. 
```csharp
var alternate = Pipe.Build<string>()
    .Use((ctx, value) => Console.WriteLine("Alternate branch executed"));

Pipe.Build<string>()
    .UseChoice((ctx, value) => value == "yes", alternate)
    .Use((ctx, value) => Console.WriteLine("Normal branch executed"));
```

Add two branches to the pipeline. Execute only one of them, 
and continue execution with the rest of the pipeline regardless of which branch was taken
```csharp
var trueBranch = Pipe.Build<string>()
    .Use((ctx, value) => Console.WriteLine("True Branch"));

var falseBranch = Pipe.Build<string>()
    .Use((ctx, value) => Console.WriteLine("False Branch"));

Pipe.Build<string>()
    .UseBranch(
        (ctx, value) => value == "yes",
        trueBranch,
        falseBranch
    )
    .Use(...); // Executed after trueBranch or falseBranch
```

#### Switch extensions
Add several branches and execute only one of them.

TODO add example code

#### Transform extensions
Add context and value transformations to the pipeline

```csharp
Pipe.Build<int>()
    .Transfrom().Use((ctx, value) => (ctx, value.ToString()))
```

## Pipe Context
A pipe's context is used to carry request-scoped information across API boundaries.

To quote the [Go context documentation](https://golang.org/pkg/context/#pkg-overview): 

>_The Context type carries deadlines, cancellation signals, and other request-scoped values across API boundaries and between processes._
>
>_Use context values only for request-scoped data that transits processes and APIs, not for passing optional parameters to functions._

In .NET this translates to:
- The cancellation token 
- Any other per-request data used by pipeline infrastructure like 
 [`ClaimsPrincipal`](https://docs.microsoft.com/en-us/dotnet/api/system.security.claims.claimsprincipal), 
 [W3C Trace Context `trace-id` or `parent-id`](https://www.w3.org/TR/trace-context/#trace-id)

>TODO add IContext usage examples and implementation hints.

## Diagnostics

The pipe's runtime structure can be walked using `IPipe.Accept(IGraphBuilder<IPipe> visitor);`  
There are some built-in graph walkers that visualize the pipe. 

>TODO add dgml graph example.

Furthermore there are the `IInspectable` and `IValidatable` 
interfaces that pipe segments can implement for more advanced runtime diagnostics

>TODO add IInspectable and IValidatable examples 

## Pipe Middleware

> TODO Write guide on how to implement middleware for maximum effect and utility

Basic anatomy of a fully implemented middleware
```csharp

public static class MyMiddleware
{
    // public facing API of your middleware
    public static IBuilder<TIn, TOut> UseYourExtension<TIn, TOut>(this IBuilder<TIn, TOut> builder, ...args for your middleware...)
    {
        // create and return your pipe builder here
    }
    
    // construction part of the middleware
    class Builder<T> : Builder, IBuilder<T, T>
    { 
    
        public Builder(...args..., string? name = null) : base(name)
        {
            // initialize builder
        }
    
        public async Task<IPipe<T>> Build(IPipe<T> next)
        {
            // create new pipe instance
            
        }
    
        public override void Accept(IGraphBuilder<IBuilder> visitor)
        {
             // add this builder's info to the graph here
        }
    }

    // execution part of the middleware
    class Pipe<T> : IPipe<T>
    { 
        private readonly string _name;
        private readonly IPipe<T> _nexxt;

        public Pipe(...args..., IPipe<T> next, string? name = null)
        {
            // initialize pipe
            _next = next;
        }

        public async Task Execute(IContext ctx, T value)
        {
            //  middleware functionality and next step here
            ...
            await _next.Execute(ctx,value);
            ...
        }

        public void Accept(IGraphBuilder<IPipe> visitor)
        {           
             // add this pipe's info to the graph here
        }
    }

}
```

# Wishlist
Unimplemented features:

- [ ] More parallel processing primitives  
  - [ ] scatter-gather ( send request to multiple servers in parallel, aggregate all responses)
  - [ ] round-robin ( distribute requests to a pool of servers) 
  - [ ] first-wins ( send request to multiple servers in parallel, first response wins )
  - [ ] ...
- [ ] Synchronous version of RedPipes ( by automatic code conversion? ) so you can use `Span<T>` or `ref struct` as the pipe arguments
- [ ] More fittings ( adapters for 3rd party libraries )
  - [ ] ASP.NET Core request handling
  - [ ] gRPC
  - [ ] ...
