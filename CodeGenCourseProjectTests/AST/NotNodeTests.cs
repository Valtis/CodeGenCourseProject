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
    public class NotNodeTests
    {
        [TestMethod()]
        public void TwoNotNodesAreEqual()
        {
            Assert.AreEqual(new NotNode(1, 3, null), new NotNode(3, 12, null));
        }

        [TestMethod()]
        public void NotNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual("<NotNode>", new NotNode(3, 12, null).ToString());
        }
    }
}