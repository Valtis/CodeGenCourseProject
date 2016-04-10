using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.Tokens;

namespace CodeGenCourseProject.AST.Tests
{
    [TestClass()]
    public class ArrayIndexTests
    {
        [TestMethod()]
        public void TwoArrayIndexNodesAreEqualWhenValuesAreEqual()
        {
            Assert.AreEqual(
                new ArrayIndex(1, 2, new IdentifierToken("hello"), null),
                new ArrayIndex(6, 32, new IdentifierToken("hello"), null));
        }

        [TestMethod()]
        public void TwoArrayIndexNodessAreNotEqualWhenValuesAreNotEqual()
        {
            Assert.AreNotEqual(
                new ArrayIndex(1, 2, new IdentifierToken("hello"), null),
                new ArrayIndex(6, 32, new IdentifierToken("world"), null));
        }

        [TestMethod()]
        public void ArrayIndexNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual(
                "<ArrayIndexNode>",
                new ArrayIndex(6, 32, new IdentifierToken("hello"), null).ToString());
        }
    }
}