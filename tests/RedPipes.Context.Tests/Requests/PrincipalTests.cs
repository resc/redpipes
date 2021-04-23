using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedPipes.Auth;
using RedPipes.Patterns.Auth;

namespace RedPipes.Requests
{
    [TestClass]
    public class PrincipalTests
    {
        [TestMethod]
        public void CanAddPrincipal()
        {
            var p = new ClaimsPrincipal(new ClaimsIdentity());
            var ctx = Context.Background.WithPrincipal(p);
            Assert.AreEqual(p, ctx.GetPrincipal());
        }

       [TestMethod]
        public void CanRemovePrincipal()
        {
            var p = new ClaimsPrincipal(new ClaimsIdentity());
            var ctx = Context.Background.WithPrincipal(p);
            Assert.AreEqual(p, ctx.GetPrincipal());

            var ctx2 = ctx.WithoutPrincipal();
            Assert.AreEqual(p, ctx.GetPrincipal());
            Assert.AreNotEqual(p, ctx2.GetPrincipal());
        }
    }
}
