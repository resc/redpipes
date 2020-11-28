using System;
using System.Security.Claims;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace RedPipes.Patterns.Auth
{
    class DelegatePrincipalProvider<T> : PrincipalProvider<T>
    {
        private readonly Func<IContext, T, Task<ClaimsPrincipal>> _provide;

        public DelegatePrincipalProvider([NotNull] Func<IContext, T, Task<ClaimsPrincipal>> provide)
        {
            _provide = provide ?? throw new ArgumentNullException(nameof(provide));
        }

        public override Task<ClaimsPrincipal> Provide(IContext ctx, T value)
        {
            return _provide(ctx, value);
        }
    }
}