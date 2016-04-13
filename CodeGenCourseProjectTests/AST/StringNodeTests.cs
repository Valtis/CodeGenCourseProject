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
    public class StringNodeTests
    {
        [TestMethod()]
        public void TwoStringNodesWithSameValueAreEqual()
        {
            Assert.AreEqual(new StringNode(43, 23, "edf"), new StringNode(2, 1, "edf"));
        }

        [TestMethod()]
        public void TwoStringNodesWithSameDifferentAreNotEqual()
        {
            Assert.AreNotEqual(new StringNode(43, 23, "dfdf"), new StringNode(2, 1, "edf"));
        }

        [TestMethod()]
        public void StringNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual("<StringNode - 'foo'>", new StringNode(0, 432, "foo").ToString());
        }
    }
}