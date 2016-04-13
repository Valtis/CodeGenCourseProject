using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.Tokens;

namespace CodeGenCourseProject.AST.Tests
{
    [TestClass()]
    public class ArrayIndexNodeTests
    {
        [TestMethod()]
        public void TwoArrayIndexNodesAreEqualWhenValuesAreEqual()
        {
            Assert.AreEqual(
                new ArrayIndexNode(1, 2, new IdentifierToken("hello"), null),
                new ArrayIndexNode(6, 32, new IdentifierToken("hello"), null));
        }

        [TestMethod()]
        public void TwoArrayIndexNodessAreNotEqualWhenValuesAreNotEqual()
        {
            Assert.AreNotEqual(
                new ArrayIndexNode(1, 2, new IdentifierToken("hello"), null),
                new ArrayIndexNode(6, 32, new IdentifierToken("world"), null));
        }

        [TestMethod()]
        public void ArrayIndexNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual(
                "<ArrayIndexNode - 'hello'>",
                new ArrayIndexNode(6, 32, new IdentifierToken("hello"), null).ToString());
        }
    }
}