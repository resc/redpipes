using System.Security.Claims;
using System.Threading.Tasks;

namespace RedPipes.Auth
{
    /// <summary>  </summary>
    public class PrincipalProvider<T>
    {
        /// <summary>  </summary>
        public virtual Task<ClaimsPrincipal> Provide(IContext ctx, T value)
        {
            return Task.FromResult(Principal.Anonymous.Clone());
        }
    }
}
