using System.Security.Claims;
using System.Threading.Tasks;

namespace RedPipes.Patterns.Auth
{
    public class PrincipalProvider<T>
    {
        public virtual Task<ClaimsPrincipal> Provide(IContext ctx, T value)
        {
            return Task.FromResult(Principal.Anonymous.Clone());
        }
    }
}