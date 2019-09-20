using Clipboard.Core.Desktop.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clipboard.Core.Desktop.Tests.ComponentModel
{
    [TestClass]
    public class CoreHelperTests
    {
        [TestMethod]
        public void GetApplicationName()
        {
            var hash = CoreHelper.GetApplicationName();

            Assert.AreEqual(hash, "UnitTestApp");
        }
    }
}
