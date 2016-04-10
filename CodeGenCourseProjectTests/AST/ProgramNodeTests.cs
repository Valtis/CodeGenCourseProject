using CodeGenCourseProject.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeGenCourseProject.AST.Tests
{
    [TestClass()]
    public class ProgramNodeTests
    {
        [TestMethod()]
        public void TwoProgramNodesWithSameIdentifierAreEqual()
        {
            Assert.AreEqual(
                new ProgramNode(0, 3, new IdentifierToken("foo"), null),
                new ProgramNode(43, 23, new IdentifierToken("foo"), null));
        }

        [TestMethod()]
        public void TwoProgramNodesWithDifferentIdentifierAreNotEqual()
        {
            Assert.AreNotEqual(
                new ProgramNode(0, 3, new IdentifierToken("foo"), null),
                new ProgramNode(43, 23, new IdentifierToken("bar"), null));
        }

        [TestMethod()]
        public void ProgramNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual(
                "<ProgramNode - 'foobar'>", 
                new ProgramNode(0, 0, new IdentifierToken("foobar"), null).ToString());
        }

    }
}