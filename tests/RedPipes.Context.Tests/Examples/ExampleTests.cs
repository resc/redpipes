using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedPipes.Tracing;

namespace RedPipes.Examples
{
    [TestClass]
    public class ExampleTests
    {

        [TestMethod]
        public async Task CanRunWithSystemDiagnosticActivity()
        {
            var sut = new WithSystemDiagnosticsActivity();

            await sut.Execute(Context.Background, new string[] { "http://www.example.com/"});

        }
    }
}
