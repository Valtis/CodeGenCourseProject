
using CodeGenCourseProject.ErrorHandling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeGenCourseProjectTests
{
    internal class TestHelper
    {
        MessageReporter reporter;
        internal TestHelper(MessageReporter reporter)
        {
            this.reporter = reporter;
        }

        internal void AssertErrorMessage(
            int errorNumber,
            MessageKind errorKind,
            int line,
            int column,
            params string [] messages)
        {
            Assert.AreEqual(errorKind, reporter.Errors[errorNumber].Type);
            Assert.AreEqual(line, reporter.Errors[errorNumber].Line);
            Assert.AreEqual(column, reporter.Errors[errorNumber].Column);

            foreach (var message in messages)
            {
                Assert.IsTrue(reporter.Errors[errorNumber].Message.Contains(message));
            }
        }
    }
}
