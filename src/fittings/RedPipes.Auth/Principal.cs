using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using RedPipes.Configuration;
using RedPipes.Configuration.Visualization;

namespace RedPipes.Patterns.Auth
{
    public static class Principal
    {
        private static readonly object _key = Context.NewKey(nameof(Principal));

        internal static readonly ClaimsPrincipal Anonymous =
            new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimsIdentity.DefaultNameClaimType, "anonymous")
                    }, null,
                    ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType));

        public static ClaimsPrincipal GetPrincipal(this IContext ctx)
        {
            if (ctx.TryGetValue(_key, out ClaimsPrincipal principal))
                return principal;

            return Anonymous.Clone();
        }

        public static IContext WithPrincipal(this IContext ctx, ClaimsPrincipal principal)
        {
            if (principal == null)
                return ctx.WithoutPrincipal();

            return ctx.With(_key, principal);
        }

        public static IContext WithoutPrincipal(this IContext ctx)
        {
            if (ctx.TryGetValue(_key, out ClaimsPrincipal _))
                return ctx.Without(_key);

            return ctx;
        }

        public static IBuilder<TIn, TOut> UsePrincipalProvider<TIn, TOut>(this IBuilder<TIn, TOut> builder, PrincipalProvider<TOut> principalProvider)
        {
            if (principalProvider == null)
                throw new ArgumentNullException(nameof(principalProvider));

            return builder.Use(new Builder<TOut>(principalProvider));
        }

        public static IBuilder<TIn, TOut> UsePrincipalProvider<TIn, TOut>(this IBuilder<TIn, TOut> builder, Func<IContext, TOut, Task<ClaimsPrincipal>> principalProvider)
        {
            if (principalProvider == null)
                throw new ArgumentNullException(nameof(principalProvider));

            return builder.Use(new Builder<TOut>(new DelegatePrincipalProvider<TOut>(principalProvider)));
        }

        class Builder<T> : Builder, IBuilder<T, T>
        {
            private readonly PrincipalProvider<T> _principalProvider;

            public Builder(PrincipalProvider<T> principalProvider)
            {
                _principalProvider = principalProvider;
            }

            public Task<IPipe<T>> Build(IPipe<T> next)
            {
                IPipe<T> pipe = new Pipe<T>(_principalProvider, next);
                return Task.FromResult(pipe);
            }
        }

        class Pipe<T> : IPipe<T>
        {
            private readonly PrincipalProvider<T> _principalProvider;
            private readonly IPipe<T> _next;

            public Pipe(PrincipalProvider<T> principalProvider, IPipe<T> next)
            {
                _principalProvider = principalProvider;
                _next = next;
            }

            public async Task Execute(IContext ctx, T value)
            {
                var p = await _principalProvider.Provide(ctx, value);
                await _next.Execute(ctx.WithPrincipal(p), value);
            }

            public void Accept(IGraphBuilder<IPipe> visitor)
            {
                if (visitor.AddEdge(this, _next, (Keys.Name, "Next")))
                    _next.Accept(visitor);
            }
        }
    }
}
