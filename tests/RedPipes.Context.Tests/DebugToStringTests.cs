using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RedPipes
{
    [TestClass]
    public class DebugToStringTests
    {
        [TestMethod]
        public void BackgroundToString()
        {
            Assert.AreEqual("BackgroundContext", Context.Background.ToString());
        }

        [TestMethod]
        public void BackgroundWithValueToString()
        {
            var key = Context.NewKey("Test");
            var value = 1;

            var toString = Context.Background.With(key, value).ToString();

            var nl = Environment.NewLine;
            Assert.AreEqual($"Key: Test, Value: 1{nl}  BackgroundContext{nl}", toString);
        }

        [TestMethod]
        public void BackgroundWithValueRemovedToString()
        {
            var key = Context.NewKey("Test");
            var value = 1;

            var toString = Context.Background
                .With(key, value)
                .Without(key)
                .ToString();

            var nl = Environment.NewLine;
            Assert.AreEqual($"Key: Test (removed){nl}  Key: Test, Value: 1{nl}    BackgroundContext{nl}", toString);

            Console.WriteLine(toString);
        }

        [TestMethod]
        public void BackgroundWithTwoValueToString()
        {
            var key = Context.NewKey("Test");
            var value = 1;

            var key2 = Context.NewKey("Test2");
            var value2 = 2;

            var toString = Context.Background
                .With(key, value)
                .With(key2, value2)
                .ToString();

            var nl = Environment.NewLine;
            Assert.AreEqual($"Key: Test2, Value: 2{nl}  Key: Test, Value: 1{nl}    BackgroundContext{nl}", toString);
        }
    }
}