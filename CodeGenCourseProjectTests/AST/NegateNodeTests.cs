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
    public class NegateNodeTests
    {
        [TestMethod()]
        public void TwoNegateNodesAreEqual()
        {
            Assert.AreEqual(new NegateNode(0, 0, null), new NegateNode(1, 5, null));
        }

        [TestMethod()]
        public void NegateNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual("<NegateNode>", new NegateNode(1, 5, null).ToString());
        }
    }
}