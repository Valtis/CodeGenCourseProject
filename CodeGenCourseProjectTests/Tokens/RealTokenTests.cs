using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace CodeGenCourseProject.Tokens.Tests
{
    [TestClass()]
    public class RealTokenTests
    {
        [TestMethod()]
        public void TwoRealsWithSameValueAreIdentical()
        {
            Assert.AreEqual(new RealToken(1234.5e2), new RealToken(1234.5e2));
        }

        [TestMethod()]
        public void TwoRealsWithDifferentValueAreNotIdentical()
        {
            Assert.AreNotEqual(new RealToken(534.2), new RealToken(1234.5e2));
        }

        [TestMethod()]
        public void RealStringRepresentationIsCorrect()
        {
            var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Assert.AreEqual("<Real - '123" + separator + "456'>", new RealToken(123.456).ToString());
            Assert.AreEqual("<Real - '1234560'>", new RealToken(123.456e4).ToString());
        }
    }
}
