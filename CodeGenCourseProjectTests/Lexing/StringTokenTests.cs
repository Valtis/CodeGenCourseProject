using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.Tokens;

namespace CodeGenCourseProject.Lexing.Tests
{
    [TestClass()]
    public class StringTokenTests
    {
        [TestMethod()]
        public void TwoStringTokensWithSameValueAreIdentical()
        {
            Assert.AreEqual(new StringToken("hello"), new StringToken("hello"));
        }

        [TestMethod()]
        public void TwoStringTokensWithDifferentValueAreNotdentical()
        {
            Assert.AreNotEqual(new StringToken("hello"), new StringToken("world"));
        }

        [TestMethod()]
        public void StringTokenStringRepresentationIsCorrect()
        {
            Assert.AreEqual("<string - 'foobar'>", new StringToken("foobar").ToString());
        }
    }
}