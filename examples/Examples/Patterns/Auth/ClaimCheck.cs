using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Patterns.Auth
{
    [Description("Some api authentication usage examples, use the first argument to set the user, and the second argument to set the user's role")]
    public class ClaimCheck : IPipe<string[]>
    {
        DateTimeOffset startDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);


        public async Task Execute(IContext c, string[] args)
        {
            var accessDenied = Pipe
                .Builder.For<Request>()
                .Use(AccessDenied);

            var builder = Pipe
                .Builder.For<string[]>()
                .Transform().Use(ArgsToRequest)
                .UsePrincipalProvider(BearerTokenPrincipalProvider)
                .UseAuthPolicy(p => p.HasRole("admin").And(p.IsAfter(startDate)), accessDenied)
                .Use(HandleRequest);

          
            var requestPipe = await builder.Build();

            await requestPipe.Execute(c, args);
        }

        private Request ArgsToRequest(string[] strings)
        {
            return new Request { Headers = { { "access_token", MakeAccessToken(strings) }, } };
        }

        private Task<ClaimsPrincipal> BearerTokenPrincipalProvider(IContext ctx, Request req)
        {
            var accessToken = req.Headers["access_token"];

            var parts = accessToken.Split(":", 2, StringSplitOptions.RemoveEmptyEntries);

            var name = parts.FirstOrDefault() ?? "anonymous";
            var role = parts.Skip(1).FirstOrDefault() ?? "anonymous";
            var authenticated = !string.IsNullOrWhiteSpace(name) && name != "anonymous";
            var authMethod = authenticated ? "DemoAuth" : null;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role),
            };

            var identity = new ClaimsIdentity(claims, authMethod);
            var principal = new ClaimsPrincipal(new[] { identity, });

            return Task.FromResult(principal);
        }

        private async Task AccessDenied(IContext ctx, Request value, Execute.AsyncFunc<Request> next)
        {
            var p = ctx.GetPrincipal();
            var role = p.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value ?? "?";
            await Console.Out.WriteLineAsync($"Access denied for user '{p.Identity.Name}' in role '{role}'");
            await next(ctx, value);
        }

        private async Task HandleRequest(IContext ctx, Request req)
        {
            var principal = ctx.GetPrincipal();
            var role = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "?";
            await Console.Out.WriteLineAsync($"User '{principal.Identity.Name}' made a request as '{role}'");
        }

        private static string MakeAccessToken(string[] strings)
        {
            return strings.FirstOrDefault() ?? "guest" + ":" + (strings.Skip(1).FirstOrDefault() ?? "anonymous");
        }

        class Request
        {
            public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        }

        public void Accept(IGraphBuilder<IPipe> visitor)
        {
            
        }
    }
}


