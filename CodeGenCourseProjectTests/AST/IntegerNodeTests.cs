using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST.Tests
{
    [TestClass()]
    public class IntegerNodeTests
    {
        [TestMethod()]
        public void TwoIntegerNodesWithSameValueAreIdentical()
        {
            Assert.AreEqual(new IntegerNode(0, 0, 5), new IntegerNode(1, 2, 5));
        }

        [TestMethod()]
        public void TwoIntegerNodeWithDifferentValueAreNotEqual()
        {
            Assert.AreNotEqual(new IntegerNode(0, 0, 5), new IntegerNode(1, 2, 9));
        }

        [TestMethod()]
        public void IntegerNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual("<IntegerNode - '5'>", new IntegerNode(0, 0, 5).ToString());
        }

    }
}