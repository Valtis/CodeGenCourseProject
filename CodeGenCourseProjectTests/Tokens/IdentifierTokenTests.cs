using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.Tokens.Tests
{
    [TestClass()]
    public class IdentifierTokenTests
    {
        [TestMethod()]
        public void IdentifiersWithIdenticalNameAreEqual()
        {
            Assert.AreEqual(new IdentifierToken("foo"), new IdentifierToken("foo"));
        }
        [TestMethod()]
        public void IdentifiersWithDifferentNameAreNotEqual()
        {
            Assert.AreNotEqual(new IdentifierToken("foo"), new IdentifierToken("bar"));
        }

        [TestMethod()]
        public void IdentifierStringRepresentationIsCorrect()
        {
            Assert.AreEqual("<identifier - 'foo'>", new IdentifierToken("foo").ToString());
        }

        [TestMethod()]
        public void IdentifierTokenStringRepresentationIsCorrectWhenNoNameIsSpecified()
        {
            Assert.AreEqual("<identifier>", new IdentifierToken().ToString());
        }

    }
}