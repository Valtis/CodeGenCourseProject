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
    public class StringTokenTests
    {
        [TestMethod()]
        public void TwoStringTokensWithSameValuesAreEqual()
        {
            Assert.AreEqual(new StringToken("hello"), new StringToken("hello"));
        }

        [TestMethod()]
        public void TwoStringTokensWithDifferentValuesAreNotEqual()
        {

            Assert.AreNotEqual(new StringToken("hello"), new StringToken("world"));
        }

        [TestMethod()]
        public void StringTokenStringRepresentationIsCorrectWithValue()
        {
            Assert.AreEqual("<string - 'foo'>", new StringToken("foo").ToString());
        }

        [TestMethod()]
        public void StringTokenStringRepresentationIsCorrectWithoutValue()
        {
            Assert.AreEqual("<string>", new StringToken().ToString());
        }
    }
}