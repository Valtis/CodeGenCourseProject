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
    public class IntegerTokenTests
    {
        [TestMethod()]
        public void TwoIntegerTokenWithSameValueAreIdentical()
        {
            Assert.AreEqual(new IntegerToken(4332), new IntegerToken(4332));
        }

        [TestMethod()]
        public void TwoIntegerTokenWithDifferentValueAreNotIdentical()
        {
            Assert.AreNotEqual(new IntegerToken(4332), new IntegerToken(434));
        }

        [TestMethod()]
        public void IntegerStringRepresentationIsCorrect()
        {
            Assert.AreEqual("<Integer - '53'>", new IntegerToken(53).ToString());
        }

    }
}