using System.IO;
using System.Text;
using Clipboard.Core.Desktop.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clipboard.Core.Desktop.Tests.IO
{
    [TestClass]
    public class LogSessionTests
    {
        [TestMethod]
        public void LogSession()
        {
            var session = new LogSession();
            session.SessionStarted();
            session.Persist(new StringBuilder("Hello World"));
            session.SessionStopped();

            Assert.AreEqual(File.ReadAllText(session.LogFilePath), "Hello World");
            Assert.IsTrue(session.LogFilePath.EndsWith("logs1"));
            Assert.AreEqual(session.GetPreviousSessionLogFilePath(), session.LogFilePath.Replace("logs1", "logs2"));

            session = new LogSession();
            session.SessionStarted();
            session.Persist(new StringBuilder("Hello World"));
            session.SessionStopped();

            Assert.IsTrue(session.LogFilePath.EndsWith("logs2"));
            Assert.AreEqual(session.GetPreviousSessionLogFilePath(), session.LogFilePath.Replace("logs2", "logs1"));

            session = new LogSession();
            session.SessionStarted();
            session.Persist(new StringBuilder("Hello World"));
            session.SessionStopped();

            Assert.IsTrue(session.LogFilePath.EndsWith("logs1"));
            Assert.AreEqual(session.GetPreviousSessionLogFilePath(), session.LogFilePath.Replace("logs1", "logs2"));

            session = new LogSession();
            session.SessionStarted();
            session.Persist(new StringBuilder("Hello World"));
            session.SessionStopped();

            Assert.IsTrue(session.LogFilePath.EndsWith("logs2"));
            Assert.AreEqual(session.GetPreviousSessionLogFilePath(), session.LogFilePath.Replace("logs2", "logs1"));
        }
    }
}
