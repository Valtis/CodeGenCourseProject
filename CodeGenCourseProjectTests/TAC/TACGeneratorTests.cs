using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.Parsing;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC.Values;

namespace CodeGenCourseProject.TAC.Tests
{
    [TestClass()]
    public class TACGeneratorTests
    {
        [TestMethod()]
        public void ExpressionsGenerateValidTAC()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\TAC\expressions.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);
            Assert.AreEqual(0, reporter.Errors.Count);

            var tacGenerator = new TACGenerator();
            node.Accept(tacGenerator);

            Assert.AreEqual(1, tacGenerator.Functions.Count);
            Assert.AreEqual(26, tacGenerator.Functions[0].Code.Count);
            var code = tacGenerator.Functions[0].Code;

            TACEquals(
                Operator.PLUS,
                new TACInteger(3),
                new TACInteger(4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 0), code[0]);

            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 0),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0), code[1]);

            TACEquals(
                Operator.MULTIPLY,
                new TACInteger(13),
                new TACInteger(20),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 1), code[2]);

            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 1),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1), code[3]);

            TACEquals(
                Operator.MINUS,
                new TACInteger(10),
                new TACInteger(7),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 2), code[4]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 2),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0), code[5]);

            TACEquals(
                Operator.DIVIDE,
                new TACInteger(1),
                new TACInteger(4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 3),
                code[6]);

            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 3),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                code[7]);

            TACEquals(
                new TACInteger(4),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0), code[8]);
            TACEquals(
                new TACInteger(-25),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1), code[9]);

            TACEquals(
                Operator.MULTIPLY,
                new TACInteger(4),
                new TACInteger(2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 4),
                code[10]);
            TACEquals(
                Operator.MINUS,
                new TACInteger(5),
                new TACInteger(9),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 5), code[11]);
            TACEquals(
                Operator.DIVIDE,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 5),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 6),
                code[12]);

            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 6),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                code[13]);

            TACEquals(
                Operator.MODULO,
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                new TACInteger(20),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 7),
                code[14]);
            TACEquals(
                Operator.MINUS,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 7),
                new TACInteger(4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 8),
                code[15]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 8),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                code[16]);

            TACEquals(
               Operator.MULTIPLY,
               new TACInteger(23),
               new TACInteger(20),
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 9),
               code[17]);
            TACEquals(
               new TACArrayDeclaration(
                   "ia", 
                   SemanticChecker.INTEGER_TYPE, 
                   new TACIdentifier("__t", 
                   SemanticChecker.INTEGER_TYPE, 9), 2),
               code[18]);

            TACEquals(
                Operator.MULTIPLY,
                new TACInteger(7),
                new TACInteger(43),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 10),
                code[19]);
            TACEquals(
                Operator.PLUS,
                new TACInteger(20),
                new TACInteger(4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 11),
                code[20]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 11),
                new TACArrayIndex(
                    "ia", 
                    new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 10),
                    SemanticChecker.INTEGER_TYPE, 2),
                code[21]);

            TACEquals(
                Operator.MODULO,
                new TACInteger(25),
                new TACInteger(2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 12), code[22]);
            TACEquals(
              Operator.MINUS,
              new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
              new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
              new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 13),
              code[23]);
            TACEquals(
                Operator.PLUS,
                new TACArrayIndex(
                    "ia",
                    new TACInteger(1),
                    SemanticChecker.INTEGER_TYPE, 2), 
                new TACArrayIndex(
                    "ia",
                    new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 13),
                    SemanticChecker.INTEGER_TYPE, 2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 14),
                code[24]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 14),
                 new TACArrayIndex(
                    "ia",
                    new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 12),
                    SemanticChecker.INTEGER_TYPE, 2),
                code[25]);
        }

        private void TACEquals(TACValue op, TACStatement actual)
        {
            Assert.AreEqual(new TACStatement(null, null, op, null), actual);
        }

        private void TACEquals(TACValue op, TACValue dest, TACStatement actual)
        {
            Assert.AreEqual(new TACStatement(null, null, op, dest), actual);
        }

        private void TACEquals(Operator op, TACValue lhs, TACValue rhs, TACValue dest, TACStatement actual)
        {
            Assert.AreEqual(new TACStatement(op, lhs, rhs, dest), actual);
        }

    }
}
