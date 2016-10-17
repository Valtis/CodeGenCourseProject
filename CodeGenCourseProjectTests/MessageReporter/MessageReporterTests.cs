using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeGenCourseProject.ErrorHandling.Tests
{
    [TestClass()]
    public class MessageReporterTests
    {
        [TestMethod()]
        public void ErrorsAreReportedInLineOrder()
        {
            var reporter = new MessageReporter();
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error2", 5, 6);
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error1", 3, 6);
            Assert.AreEqual(2, reporter.Errors.Count);
            Assert.AreEqual("error1", reporter.Errors[0].Message);
            Assert.AreEqual("error2", reporter.Errors[1].Message);
        }

        [TestMethod()]
        public void ErrorsAreReportedInColumOrder()
        {
            var reporter = new MessageReporter();
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error3", 5, 6);
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error2", 3, 6);
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error1", 3, 2);
            Assert.AreEqual(3, reporter.Errors.Count);
            Assert.AreEqual("error1", reporter.Errors[0].Message);
            Assert.AreEqual("error2", reporter.Errors[1].Message);
            Assert.AreEqual("error3", reporter.Errors[2].Message);
        }

        [TestMethod()]
        public void NotesAttachedToErrorsAreSortedCorrectly()
        {
            var reporter = new MessageReporter();
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error3", 5, 6);
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error2", 3, 6);
            reporter.ReportError(MessageKind.NOTE, "this is a note", 1, 5);
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error1", 3, 2);
            Assert.AreEqual(4, reporter.AllMessages.Count);
            Assert.AreEqual("error1", reporter.AllMessages[0].Message);
            Assert.AreEqual("error2", reporter.AllMessages[1].Message);
            Assert.AreEqual("this is a note", reporter.AllMessages[2].Message);
            Assert.AreEqual("error3", reporter.AllMessages[3].Message);
        }

        [TestMethod()]
        public void NotesAttachedToWarningsAreSortedCorrectly()
        {
            var reporter = new MessageReporter();
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error2", 5, 6);
            reporter.ReportError(MessageKind.WARNING, "warning1", 3, 6);
            reporter.ReportError(MessageKind.NOTE, "this is a note", 1, 5);
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error1", 3, 2);
            Assert.AreEqual(4, reporter.AllMessages.Count);
            Assert.AreEqual("error1", reporter.AllMessages[0].Message);
            Assert.AreEqual("warning1", reporter.AllMessages[1].Message);
            Assert.AreEqual("this is a note", reporter.AllMessages[2].Message);
            Assert.AreEqual("error2", reporter.AllMessages[3].Message);
        }

        [TestMethod()]
        public void MultipleNotesMaintainOrder()
        {
            var reporter = new MessageReporter();
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error2", 5, 6);
            reporter.ReportError(MessageKind.WARNING, "warning1", 3, 6);
            reporter.ReportError(MessageKind.NOTE, "this is a note 1", 2, 5);
            reporter.ReportError(MessageKind.NOTE, "this is a note 2", 1, 5);
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error1", 3, 2);
            Assert.AreEqual(5, reporter.AllMessages.Count);
            Assert.AreEqual("error1", reporter.AllMessages[0].Message);
            Assert.AreEqual("warning1", reporter.AllMessages[1].Message);
            Assert.AreEqual("this is a note 1", reporter.AllMessages[2].Message);
            Assert.AreEqual("this is a note 2", reporter.AllMessages[3].Message);
            Assert.AreEqual("error2", reporter.AllMessages[4].Message);
        }

        [TestMethod()]
        public void NotesThatAreAfterWrongErrorMessageAfterSortingDoBreakCorrectOrder()
        {
            var reporter = new MessageReporter();
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error2", 6, 1);
            reporter.ReportError(MessageKind.NOTE, "Note for error2", 4, 1);
            reporter.ReportError(MessageKind.LEXICAL_ERROR, "error1", 3, 1);
            reporter.ReportError(MessageKind.NOTE, "Note for error1", 1, 1);

            Assert.AreEqual(4, reporter.AllMessages.Count);
            Assert.AreEqual("error1", reporter.AllMessages[0].Message);
            Assert.AreEqual("Note for error1", reporter.AllMessages[1].Message);
            Assert.AreEqual("error2", reporter.AllMessages[2].Message);
            Assert.AreEqual("Note for error2", reporter.AllMessages[3].Message);
        }

    }
}