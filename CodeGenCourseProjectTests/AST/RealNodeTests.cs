using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace CodeGenCourseProject.AST.Tests
{
    [TestClass()]
    public class RealNodeTests
    {
        [TestMethod()]
        public void TwoRealNodesWithSameValueAreIdentical()
        {
            Assert.AreEqual(new RealNode(2, 5, 212.56), new RealNode(0, 23, 212.56));
        }

        [TestMethod()]
        public void TwoRealNodesWithDifferentValueAreNotIdentical()
        {
            Assert.AreNotEqual(new RealNode(2, 5, 212.56), new RealNode(0, 23, 0));
        }

        [TestMethod()]
        public void RealNodeStringRepresentationIsCorrect()
        {
            var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Assert.AreEqual("<RealNode - '212" + separator + "56'>", new RealNode(0, 23, 212.56).ToString());
        }
    }
}