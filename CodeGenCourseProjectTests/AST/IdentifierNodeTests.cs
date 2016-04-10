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
    public class IdentifierNodeTests
    {
        [TestMethod()]
        public void TwoIdentifierNodesWithSameValueAreIdentical()
        {
            Assert.AreEqual(new IdentifierNode(0, 1, "hello"), new IdentifierNode(354, 12, "hello"));
        }

        [TestMethod()]
        public void TwoIdentifierNodesWithDifferentValueAreNotIdentical()
        {
            Assert.AreNotEqual(new IdentifierNode(0, 1, "hello"), new IdentifierNode(354, 12, "world"));
        }

        [TestMethod()]
        public void IdentifierNodeStringRepresentationIsCorrect()
        {
            Assert.AreEqual("<IdentifierNode - 'hello'>", new IdentifierNode(0, 1, "hello").ToString());
        }
    }
}