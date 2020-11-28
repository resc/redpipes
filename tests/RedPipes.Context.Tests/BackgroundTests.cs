using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedPipes
{
    [TestClass]
    public class BackgroundTests
    {
        [TestMethod]
        public void BackgroundIsNotNull()
        {
            Assert.IsNotNull(Context.Background);
        }

        [TestMethod]
        public void BackgroundCancellationTokenCanNotBeCancelled()
        {
            Assert.IsFalse(Context.Background.Token.CanBeCanceled);
        }

        //[TestMethod]
        //public void BackgroundCancellationTokenWaitHandleIsNull()
        //{
        //    Assert.IsNull(Context.Background.Token.WaitHandle);
        //}
    }
}
