using CodeGenCourseProject.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeGenCourseProject.AST.Tests
{
    [TestClass()]
    public class VariableAssignmentNodeTests
    {
        [TestMethod()]
        public void TwoVariableAssignmentNodesWithSameIdentifierAreEqual()
        {
            Assert.AreEqual(
                new VariableAssignmentNode(0, 1, new IdentifierToken("hello"), null),
                new VariableAssignmentNode(67, 45, new IdentifierToken("hello"), null));
        }

        [TestMethod()]
        public void TwoVariableAssignmentNodesWithDifferentIdentifierAreNotEqual()
        {
            Assert.AreNotEqual(
                new VariableAssignmentNode(0, 1, new IdentifierToken("hello"), null),
                new VariableAssignmentNode(67, 45, new IdentifierToken("world"), null));
        }

        [TestMethod()]
        public void VariableAssignmentNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual(
                "<AssignmentNode - 'world'>",
                new VariableAssignmentNode(67, 45, new IdentifierToken("world"), null).ToString());
        }
    }
}