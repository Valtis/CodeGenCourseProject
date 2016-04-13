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
                new VariableAssignmentNode(0, 1, null, null),
                new VariableAssignmentNode(67, 45, null, null));
        }

        [TestMethod()]
        public void TwoVariableAssignmentNodesWithDifferentIdentifierAreNotEqual()
        {
            Assert.AreNotEqual(
                new VariableAssignmentNode(0, 1, null, null),
                new VariableAssignmentNode(67, 45, null, null));
        }

        [TestMethod()]
        public void VariableAssignmentNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual(
                "<AssignmentNode>",
                new VariableAssignmentNode(67, 45, null, null).ToString());
        }
    }
}