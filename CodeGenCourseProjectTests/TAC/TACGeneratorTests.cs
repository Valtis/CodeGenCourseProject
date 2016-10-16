using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.Parsing;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC.Values;
using System.Collections.Generic;

namespace CodeGenCourseProject.TAC.Tests
{
    [TestClass()]
    public class TACGeneratorTests
    {
        private IList<Function> GetFunctions(string name, int functions)
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\TAC\" + name, reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);
            Assert.AreEqual(0, reporter.Errors.Count);

            var tacGenerator = new Generator();
            node.Accept(tacGenerator);

            Assert.AreEqual(functions, tacGenerator.Functions.Count);
            return tacGenerator.Functions;
        }
        
        [TestMethod()]
        public void ExpressionsGenerateValidTAC()
        {

            var functions = GetFunctions("expressions.txt", 1);
            Assert.AreEqual(92, functions[0].Statements.Count);
            var statements = functions[0].Statements;

            TACEquals(
                Operator.PLUS,
                new TACInteger(3),
                new TACInteger(4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 0), statements[0]);

            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 0),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0), statements[1]);

            TACEquals(
                Operator.MULTIPLY,
                new TACInteger(13),
                new TACInteger(20),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 1), statements[2]);

            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 1),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1), statements[3]);

            TACEquals(
                Operator.MINUS,
                new TACInteger(10),
                new TACInteger(7),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 2), statements[4]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 2),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0), statements[5]);

            TACEquals(
                Operator.DIVIDE,
                new TACInteger(1),
                new TACInteger(4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 3),
                statements[6]);

            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 3),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                statements[7]);

            TACEquals(
                new TACInteger(4),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0), statements[8]);
            TACEquals(
                new TACInteger(-25),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1), statements[9]);

            TACEquals(
                Operator.MULTIPLY,
                new TACInteger(4),
                new TACInteger(2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 4),
                statements[10]);
            TACEquals(
                Operator.MINUS,
                new TACInteger(5),
                new TACInteger(9),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 5), statements[11]);
            TACEquals(
                Operator.DIVIDE,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 5),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 6),
                statements[12]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 6),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                statements[13]);
            TACEquals(
                Operator.MODULO,
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                new TACInteger(20),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 7),
                statements[14]);
            TACEquals(
                Operator.MINUS,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 7),
                new TACInteger(4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 8),
                statements[15]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 8),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                statements[16]);
            TACEquals(
               Operator.MULTIPLY,
               new TACInteger(23),
               new TACInteger(20),
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 9),
               statements[17]);
            TACEquals(
               Operator.DECLARE_ARRAY,
               new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 2),
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 9),
               null,
               statements[18]);
            TACEquals(
                Operator.MULTIPLY,
                new TACInteger(7),
                new TACInteger(43),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 10),
                statements[19]);
            TACEquals(
               Operator.VALIDATE_INDEX,
               new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 2),
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 10),
               null,
               statements[20]);
            TACEquals(
              Operator.PLUS,
              new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 2),
              new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 10),
              new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 11),
              statements[21]);
            TACEquals(
                Operator.PLUS,
                new TACInteger(20),
                new TACInteger(4),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 12),
                statements[22]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 12),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 11),
                statements[23]);
            TACEquals(
                Operator.MODULO,
                new TACInteger(25),
                new TACInteger(2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 13),
                statements[24]);
            TACEquals(
                Operator.VALIDATE_INDEX,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 13),
                null,
                statements[25]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 13),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 14),
                statements[26]);
            TACEquals(
                Operator.VALIDATE_INDEX,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 2),
                new TACInteger(1),
                null,
                statements[27]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 2),
                new TACInteger(1),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 15),
                statements[28]);
            TACEquals(
                Operator.MINUS,
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 16),
                statements[29]);
            TACEquals(
                Operator.VALIDATE_INDEX,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 16),
                null,
                statements[30]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 16),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 17),
                statements[31]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 15),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 17),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 18),
                statements[32]);
            TACEquals(
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 18),
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 14),
               statements[33]);
            TACEquals(
                new TACReal(42.1),
                new TACIdentifier("k", SemanticChecker.REAL_TYPE, 3),
                statements[34]);
            TACEquals(
                new TACReal(0.0),
                new TACIdentifier("l", SemanticChecker.REAL_TYPE, 4),
                statements[35]);
            TACEquals(
                Operator.MULTIPLY,
                new TACReal(4.2),
                new TACIdentifier("l", SemanticChecker.REAL_TYPE, 4),
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 19),
                statements[36]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 19),
                new TACIdentifier("k", SemanticChecker.REAL_TYPE, 3),
                statements[37]);
            TACEquals(
                Operator.DIVIDE,
                new TACReal(3.14159),
                new TACIdentifier("k", SemanticChecker.REAL_TYPE, 3),
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 20),
                statements[38]);            
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 20),
                new TACIdentifier("l", SemanticChecker.REAL_TYPE, 4),
                statements[39]);
            TACEquals(
                Operator.MINUS,
                new TACIdentifier("l", SemanticChecker.REAL_TYPE, 4),
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 21),
                statements[40]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 21),
                new TACIdentifier("l", SemanticChecker.REAL_TYPE, 4),
                statements[41]);            
            TACEquals(
                Operator.MINUS,
                new TACReal(1.23),
                new TACReal(32.1),
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 22),
                statements[42]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 22),
                new TACIdentifier("k", SemanticChecker.REAL_TYPE, 3),
                statements[43]);
            TACEquals(
               Operator.DECLARE_ARRAY,
               new TACIdentifier("ir", SemanticChecker.REAL_TYPE, 5),
               new TACInteger(0),
               null,
               statements[44]);
            TACEquals(
                Operator.VALIDATE_INDEX,
                new TACIdentifier("ir", SemanticChecker.REAL_TYPE, 5),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                null,
                statements[45]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ir", SemanticChecker.REAL_TYPE, 5),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 23),
                statements[46]);
            TACEquals(
                Operator.MINUS,
                new TACReal(9.23),
                new TACReal(1.2),
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 24),
                statements[47]);
            TACEquals(
                Operator.DIVIDE,
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 24),
                new TACIdentifier("k", SemanticChecker.REAL_TYPE, 3),
                new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 25),
                statements[48]);
            TACEquals(
               new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 25),
               new TACIdentifier("__t", SemanticChecker.REAL_TYPE, 23),
               statements[49]);
            TACEquals(
                new TACBoolean(true),
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                statements[50]);
            TACEquals(
                new TACBoolean(false),
                new TACIdentifier("n", SemanticChecker.BOOLEAN_TYPE, 7),
                statements[51]);
            TACEquals(
                Operator.LESS_THAN,
                new TACInteger(1),
                new TACInteger(2),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 26),
                statements[52]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 26),
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                statements[53]);
                 TACEquals(
            Operator.LESS_THAN_OR_EQUAL,
                new TACReal(4.323),
                new TACReal(32.13),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 27),
                statements[54]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 27),
                new TACIdentifier("n", SemanticChecker.BOOLEAN_TYPE, 7),
                statements[55]);
            TACEquals(
                Operator.EQUAL,
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 28),
                statements[56]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 28),
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                statements[57]);
            TACEquals(
                Operator.GREATER_THAN_OR_EQUAL,
                new TACInteger(1),
                new TACInteger(2),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 29),
                statements[58]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 29),
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                statements[59]);            
            TACEquals(
                Operator.GREATER_THAN,
                new TACInteger(1),
                new TACInteger(2),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 30),
                statements[60]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 30),
                new TACIdentifier("n", SemanticChecker.BOOLEAN_TYPE, 7),
                statements[61]);
            TACEquals(
                Operator.AND,
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                new TACBoolean(true),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 31),
                statements[62]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 31),
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                statements[63]);
            TACEquals(
                Operator.OR,
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                new TACBoolean(false),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 32),
                statements[64]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 32),
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                statements[65]);
            TACEquals(
                Operator.NOT,
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 33),
                statements[66]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 33),
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                statements[67]);
            TACEquals(
                new TACIdentifier("true", SemanticChecker.BOOLEAN_TYPE, 8),
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                statements[68]);
            TACEquals(
                new TACIdentifier("false", SemanticChecker.BOOLEAN_TYPE, 9),
                new TACIdentifier("n", SemanticChecker.BOOLEAN_TYPE, 7),
                statements[69]);
            TACEquals(
               Operator.DECLARE_ARRAY,
               new TACIdentifier("ib", SemanticChecker.BOOLEAN_TYPE, 10),
               new TACInteger(99),
               null,
               statements[70]);
            TACEquals(
                Operator.VALIDATE_INDEX,
                new TACIdentifier("ib", SemanticChecker.BOOLEAN_TYPE, 10),
                new TACInteger(21),
                null,
                statements[71]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ib", SemanticChecker.BOOLEAN_TYPE, 10),
                new TACInteger(21),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 34),
                statements[72]);
            TACEquals(
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 34),
                statements[73]);
            TACEquals(
                new TACString("hello "),
                new TACIdentifier("o", SemanticChecker.STRING_TYPE, 11),
                statements[74]);            
            TACEquals(
                new TACString("world"),
                new TACIdentifier("p", SemanticChecker.STRING_TYPE, 12),
                statements[75]);            
            TACEquals(
                Operator.CONCAT,
                new TACIdentifier("o", SemanticChecker.STRING_TYPE, 11),
                new TACIdentifier("p", SemanticChecker.STRING_TYPE, 12),
                new TACIdentifier("__t", SemanticChecker.STRING_TYPE, 35),
                statements[76]);            
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.STRING_TYPE, 35),
                new TACIdentifier("o", SemanticChecker.STRING_TYPE, 11),
                statements[77]);
            TACEquals(
               Operator.DECLARE_ARRAY,
               new TACIdentifier("is", SemanticChecker.STRING_TYPE, 13),
               new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
               null,
               statements[78]);
            TACEquals(
                Operator.VALIDATE_INDEX,
                new TACIdentifier("is", SemanticChecker.STRING_TYPE, 13),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                null,
                statements[79]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("is", SemanticChecker.STRING_TYPE, 13),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                new TACIdentifier("__t", SemanticChecker.STRING_TYPE, 36),
                statements[80]);
            TACEquals(
                new TACIdentifier("o", SemanticChecker.STRING_TYPE, 11),
                new TACIdentifier("__t", SemanticChecker.STRING_TYPE, 36),
                statements[81]);
            TACEquals(
               Operator.DECLARE_ARRAY,
               new TACIdentifier("ia2", SemanticChecker.INTEGER_TYPE, 14),
               new TACInteger(0),
               null,
               statements[82]);
            TACEquals(
                new TACIdentifier("ia2", "Array<" + SemanticChecker.INTEGER_TYPE + ">", 14),
                new TACIdentifier("ia", "Array<" + SemanticChecker.INTEGER_TYPE + ">", 2),
                statements[83]);
            TACEquals(
                Operator.ARRAY_SIZE,
                null,
                new TACIdentifier("ia", "Array<" + SemanticChecker.INTEGER_TYPE + ">", 2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 37),
                statements[84]);
            TACEquals(
                Operator.PLUS,
                new TACInteger(2),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 37),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 38),
                statements[85]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 38),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                statements[86]);
            TACEquals(
                Operator.NOT_EQUAL,
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                new TACIdentifier("j", SemanticChecker.INTEGER_TYPE, 1),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 39),
                statements[87]); 
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 39),
                new TACIdentifier("m", SemanticChecker.BOOLEAN_TYPE, 6),
                statements[88]);
            TACEquals(
                new TACInteger(4),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                statements[89]);
            TACEquals(
                new TACString("hello"),
                new TACIdentifier("i", SemanticChecker.STRING_TYPE, 15),
                statements[90]);
            TACEquals(
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                new TACIdentifier("i", SemanticChecker.INTEGER_TYPE, 0),
                statements[91]);
        }
        
        [TestMethod()]
        public void WhileStatementsGenerateValidTAC()
        {
            var functions = GetFunctions("while_statements.txt", 1);
            var statements = functions[0].Statements;

            Assert.AreEqual(60, statements.Count);

            TACEquals(
                Operator.LABEL,
                new TACInteger(0),
                null,
                statements[0]);
            TACEquals(
                Operator.JUMP_IF_FALSE,
                new TACBoolean(true),
                new TACInteger(1),
                null,
                statements[1]);
            TACEquals(
                new TACInteger(4),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 0),
                statements[2]);
            TACEquals(
                Operator.JUMP,
                new TACInteger(0),
                null,               
                statements[3]);
            TACEquals(
                Operator.LABEL,
                new TACInteger(1),
                null,
                statements[4]);
            TACEquals(
                Operator.LABEL,
                new TACInteger(2),
                null,
                statements[5]);
            TACEquals(
                Operator.JUMP_IF_FALSE,
                new TACBoolean(false),
                new TACInteger(3),
                null,
                statements[6]);
            TACEquals(
                Operator.JUMP,
                new TACInteger(2),
                null,
                statements[7]);
            TACEquals(
                Operator.LABEL,
                new TACInteger(3),
                null,
                statements[8]);            
            TACEquals(
                new TACInteger(0),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 2),
                statements[9]);
            TACEquals(
                Operator.LABEL,
                new TACInteger(4),
                null,
                statements[10]);            
            TACEquals(
                Operator.LESS_THAN,
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 2),
                new TACInteger(5),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 0),
                statements[11]);
            TACEquals(
                Operator.JUMP_IF_FALSE,
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 0),
                new TACInteger(5),
                null,
                statements[12]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 2),
                new TACInteger(1),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 1),
                statements[13]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 1),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 2),
                statements[14]);
            TACEquals(
                Operator.PUSH,
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 2),
                null,
                statements[15]
                );
            TACEquals(
                Operator.CALL_WRITELN,
                new TACInteger(1),
                null,
                statements[16]);
            TACEquals(
                Operator.JUMP,
                new TACInteger(4),
                null,
                statements[17]);
            TACEquals(
                Operator.LABEL,
                new TACInteger(5),
                null,
                statements[18]);            
            TACEquals(
                new TACInteger(0),
                new TACIdentifier("b", SemanticChecker.INTEGER_TYPE, 3),
                statements[19]);
            TACEquals(
               Operator.LABEL,
               new TACInteger(6),
               null,
               statements[20]);
            TACEquals(
                Operator.LESS_THAN_OR_EQUAL,
                new TACIdentifier("b", SemanticChecker.INTEGER_TYPE, 3),
                new TACInteger(5),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 2),
                statements[21]);
            TACEquals(
                Operator.JUMP_IF_FALSE,
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 2),
                new TACInteger(7),
                null,
                statements[22]);
            TACEquals(
                new TACInteger(0),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 4),
                statements[23]);
            TACEquals(
                Operator.PUSH,
                new TACInteger(1),
                null,
                statements[24]);
            TACEquals(
                Operator.CALL_WRITELN,                
                new TACInteger(1),
                null,
                statements[25]);
            TACEquals(
               Operator.LABEL,
               new TACInteger(8),
               null,
               statements[26]);
            TACEquals(
                Operator.LESS_THAN,
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 4),
                new TACInteger(3),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 3),
                statements[27]);
            TACEquals(
                Operator.JUMP_IF_FALSE,
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 3),
                new TACInteger(9),
                null,
                statements[28]);
            TACEquals(
                Operator.PUSH,
                new TACInteger(2),
                null,
                statements[29]);
            TACEquals(
                Operator.CALL_WRITELN,
                new TACInteger(1), 
                null,
                statements[30]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 4),
                new TACInteger(1),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 4),
                statements[31]
                );
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 4),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 4),
                statements[32]);
            TACEquals(
                Operator.JUMP,
                new TACInteger(8),
                null,
                statements[33]);
            TACEquals(
               Operator.LABEL,
               new TACInteger(9),
               null,
               statements[34]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("b", SemanticChecker.INTEGER_TYPE, 3),
                new TACInteger(1),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 5),
                statements[35]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 5),
                new TACIdentifier("b", SemanticChecker.INTEGER_TYPE, 3),
                statements[36]);
            TACEquals(
                Operator.JUMP,
                new TACInteger(6),
                null,
                statements[37]);
            TACEquals(
               Operator.LABEL,
               new TACInteger(7),
               null,
               statements[38]);
            TACEquals(
               Operator.DECLARE_ARRAY,
               new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
               new TACInteger(1),
               null,
               statements[39]);
            TACEquals(
                Operator.VALIDATE_INDEX,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
                new TACInteger(0),
                null,
                statements[40]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
                new TACInteger(0),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 6),
                statements[41]);
            TACEquals(
                new TACInteger(0),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 6),
                statements[42]);            
            TACEquals(
                Operator.LABEL,
                new TACInteger(10),
                null,
                statements[43]);
            TACEquals(
                Operator.VALIDATE_INDEX,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
                new TACInteger(0),
                null,
                statements[44]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
                new TACInteger(0),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 7),
                statements[45]);
            TACEquals(
                Operator.LESS_THAN,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 7),
                new TACInteger(5),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 8),
                statements[46]);
            TACEquals(
                Operator.JUMP_IF_FALSE,
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 8),
                new TACInteger(11),
                null,
                statements[47]);
            TACEquals(
               Operator.VALIDATE_INDEX,
               new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
               new TACInteger(0),
               null,
               statements[48]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
                new TACInteger(0),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 9),
                statements[49]);
            TACEquals(
                Operator.PUSH,
                null,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 9),
                null,
                statements[50]);
            TACEquals(
                Operator.CALL_WRITELN,
                new TACInteger(1),
                null,
                statements[51]);
            TACEquals(
               Operator.VALIDATE_INDEX,
               new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
               new TACInteger(0),
               null,
               statements[52]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
                new TACInteger(0),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 10),
                statements[53]);
            TACEquals(
               Operator.VALIDATE_INDEX,
               new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
               new TACInteger(0),
               null,
               statements[54]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("ia", SemanticChecker.INTEGER_TYPE, 5),
                new TACInteger(0),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 11),
                statements[55]);
            TACEquals(
                Operator.PLUS,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 11),
                new TACInteger(1),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 12),
                statements[56]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 12),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 10),
                statements[57]);
            TACEquals(
                Operator.JUMP,
                new TACInteger(10),
                null,
                statements[58]);
            TACEquals(
               Operator.LABEL,
               new TACInteger(11),
               null,
               statements[59]);
        }

        [TestMethod()]
        public void IfStatementsGenerateValidTAC()
        {
            var functions = GetFunctions("if_statements.txt", 1);
            var statements = functions[0].Statements;

            Assert.AreEqual(10, statements.Count);
            TACEquals(
                Operator.JUMP_IF_FALSE,
                new TACBoolean(true),
                new TACInteger(0),
                null,
                statements[0]);
            TACEquals(
                new TACInteger(4),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 0),
                statements[1]);
            TACEquals(
               Operator.LABEL,
               new TACInteger(0),
               null,
               statements[2]);
            TACEquals(
                Operator.LESS_THAN,
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 0),
                new TACInteger(4),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 0),
                statements[3]);
            TACEquals(
                Operator.JUMP_IF_FALSE,
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 0),
                new TACInteger(1),
                null,
                statements[4]);            
            TACEquals(
                new TACInteger(6),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 0),
                statements[5]);
            TACEquals(
                Operator.JUMP,
                new TACInteger(2),
                null,
                statements[6]);
            TACEquals(
               Operator.LABEL,
               new TACInteger(1),
               null,
               statements[7]);
            TACEquals(
                new TACInteger(2),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 0),
                statements[8]);
            TACEquals(
               Operator.LABEL,
               new TACInteger(2),
               null,
               statements[9]);
        }
        
        [TestMethod()]
        public void ProceduresFunctionsAndFunctionCallsGenerateValidTAC()
        {
            var functions = GetFunctions("functions_and_procedures.txt", 8);

            Assert.AreEqual(Helper.MangleFunctionName("proc_1", 4), functions[0].Name);
            var statements = functions[0].Statements;
            Assert.AreEqual(6, statements.Count);

            TACEquals(
               Operator.MULTIPLY,
               new TACInteger(3),
               new TACInteger(2),
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 1),
               statements[0]);
            TACEquals(
               Operator.MINUS,
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 1),
               new TACInteger(1),
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 2),
               statements[1]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 2),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 5),
                statements[2]);
            TACEquals(
               Operator.OR,
               new TACBoolean(true),
               new TACBoolean(false),
               new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 3),
               statements[3]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 3),
                new TACIdentifier("c", SemanticChecker.BOOLEAN_TYPE, 7),
                statements[4]);
            TACEquals(
                Operator.RETURN,
                null,
                null, 
                null,
                statements[5]);

            Assert.AreEqual(Helper.MangleFunctionName("func_1", 8), functions[1].Name);
            statements = functions[1].Statements;
            Assert.AreEqual(7, statements.Count);
            TACEquals(
                Operator.MULTIPLY,
                new TACInteger(2),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 9),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 4),
                statements[0]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 4),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 9),
                statements[1]);
            TACEquals(
                 Operator.GREATER_THAN,
                 new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 9),
                 new TACInteger(4),
                 new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 5),
                 statements[2]);
            TACEquals(
                Operator.JUMP_IF_FALSE,
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 5),
                new TACInteger(0),
                null,
                statements[3]);
            TACEquals(
                Operator.RETURN,
                null,
                new TACReal(23.12),
                null,
                statements[4]);   
            TACEquals(
               Operator.LABEL,
               new TACInteger(0),
               null,
               statements[5]);
            TACEquals(
                Operator.RETURN,
                null,
                new TACReal(0.0),
                null,
                statements[6]);

            Assert.AreEqual(Helper.MangleFunctionName("writeln", 11), functions[2].Name);
            statements = functions[2].Statements;
            Assert.AreEqual(1, statements.Count);
            TACEquals(
                Operator.RETURN,
                null,
                null,
                null,
                statements[0]);

            Assert.AreEqual(Helper.MangleFunctionName("read", 12), functions[3].Name);
            statements = functions[3].Statements;
            Assert.AreEqual(1, statements.Count);
            TACEquals(
                Operator.RETURN,
                null,
                null,
                null,
                statements[0]);

            Assert.AreEqual(Helper.MangleFunctionName("func_2", 14), functions[4].Name);
            statements = functions[4].Statements;
            Assert.AreEqual(1, statements.Count);
            TACEquals(
                Operator.RETURN,
                null,
                new TACInteger(4),
                null,
                statements[0]);

            Assert.AreEqual(Helper.MangleFunctionName("func_3", 16), functions[5].Name);
            statements = functions[5].Statements;
            Assert.AreEqual(1, statements.Count);
            TACEquals(
                Operator.RETURN,
                null,
                null,
                null,
                statements[0]);

            Assert.AreEqual(Helper.MangleFunctionName("func_4", 18), functions[6].Name);
            statements = functions[6].Statements;
            Assert.AreEqual(1, statements.Count);
            TACEquals(
                Operator.RETURN,
                null,
                null,
                null,
                statements[0]);

            Assert.AreEqual("<ENTRY POINT>", functions[7].Name);
            statements = functions[7].Statements;
            Assert.AreEqual(33, statements.Count);
            TACEquals(
                Operator.MULTIPLY,
                new TACInteger(1),
                new TACInteger(3),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 0),
                statements[0]);

            TACEquals(
                Operator.PUSH,
                new TACReal(123.45),
                null,
                statements[1]);
            TACEquals(
                Operator.PUSH,
                new TACString("hello"),
                null,
                statements[2]);
            TACEquals(
                Operator.PUSH,
                new TACBoolean(true),
                null,
                statements[3]);
            TACEquals(
                Operator.PUSH,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 0),
                null,
                statements[4]);
            TACEquals(
                Operator.CALL_WRITELN,
                new TACInteger(4),
                null,
                statements[5]);

            TACEquals(
                Operator.PUSH_INITIALIZED,
                new TACIdentifier("d", SemanticChecker.REAL_TYPE, 3),
                null,
                statements[6]);
            TACEquals(
                Operator.PUSH_INITIALIZED,
                new TACIdentifier("c", SemanticChecker.STRING_TYPE, 2),
                null,
                statements[7]);
            TACEquals(
                 Operator.PUSH_INITIALIZED,
                 new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 0),
                 null,
                 statements[8]);
            TACEquals(
                Operator.CALL_READ,
                new TACInteger(3),
                null,
                statements[9]);

            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("writeln", 11),
                null,
                statements[10]);

            TACEquals(
                 Operator.PUSH,
                 new TACString("hello"),
                 null,
                 statements[11]);
            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("read", 12),
                null,
                statements[12]);
            TACEquals(
                Operator.PUSH,
                new TACInteger(23),
                null,
                statements[13]);
            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("func_2", 14),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 6),
                statements[14]);
            TACEquals(
                Operator.PUSH,
                new TACInteger(32),
                null,
                statements[15]);
            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("func_2", 14),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 7),
                statements[16]);

            TACEquals(
                Operator.PUSH,
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 7),
                null,
                statements[17]);
            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("func_2", 14),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 8),
                statements[18]);
            TACEquals(
                Operator.PUSH,
                new TACInteger(23),
                null,
                statements[19]);
            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("func_2", 14),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 9),
                statements[20]);
            TACEquals(
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 9),
               new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 0),
               statements[21]);

            TACEquals(
                Operator.PUSH,
                new TACInteger(32),
                null,
                statements[22]);
            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("func_2", 14),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 10),
                statements[23]);
            TACEquals(
               Operator.PUSH,
               new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 10),
               null,
               statements[24]);
            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("func_2", 14),
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 11),
                statements[25]);
            TACEquals(
                new TACIdentifier("__t", SemanticChecker.INTEGER_TYPE, 11),
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 0),
                statements[26]);
            TACEquals(
               Operator.DECLARE_ARRAY,
               new TACIdentifier("x", SemanticChecker.INTEGER_TYPE, 20),
               new TACInteger(4),
               null,
               statements[27]);
            TACEquals(
                Operator.CLONE_ARRAY,
                null,
                new TACIdentifier("x", SemanticChecker.INTEGER_ARRAY, 20),
                new TACIdentifier("__t", SemanticChecker.INTEGER_ARRAY, 12),
                statements[28]);

            TACEquals(
                Operator.PUSH,
                new TACIdentifier("__t", SemanticChecker.INTEGER_ARRAY, 12),
                null,
                statements[29]);

            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("func_3", 16),
                null,
                statements[30]);

            TACEquals(
                Operator.PUSH,
                new TACIdentifier("x", SemanticChecker.INTEGER_ARRAY, 20),
                null,
                statements[31]);
            
            TACEquals(
                Operator.CALL,
                new TACFunctionIdentifier("func_4", 18),
                null,
                statements[32]);
        }
       
        [TestMethod()]
        public void AssertsGenerateCorrectTAC()
        {
            var functions = GetFunctions("asserts.txt", 1);
            var statements = functions[0].Statements;

            Assert.AreEqual(4, statements.Count);

            TACEquals(
                Operator.LESS_THAN,
                new TACIdentifier("a", SemanticChecker.INTEGER_TYPE, 0),
                new TACInteger(2),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 0),
                statements[0]);
            TACEquals(
                Operator.CALL_ASSERT,
                new TACInteger(3),
                new TACIdentifier("__t", SemanticChecker.BOOLEAN_TYPE, 0),
                null,
                statements[1]);
            TACEquals(
               Operator.CALL_ASSERT,
               new TACInteger(4),
               new TACBoolean(true),
               null,
               statements[2]);
            TACEquals(
               Operator.CALL_ASSERT,
               new TACInteger(5),
               new TACBoolean(false),
               null,
               statements[3]);
        }

        private void TACEquals(TACValue value, Statement actual)
        {
            Assert.AreEqual(new Statement(null, null, value, null), actual);
        }

        private void TACEquals(TACValue value, TACValue dest, Statement actual)
        {
            Assert.AreEqual(new Statement(null, null, value, dest), actual);
        }

        private void TACEquals(Operator op, TACValue rhs, TACValue dest, Statement actual)
        {
            Assert.AreEqual(new Statement(op, null, rhs, dest), actual);
        }
        private void TACEquals(Operator op, TACValue lhs, TACValue rhs, TACValue dest, Statement actual)
        {
            Assert.AreEqual(new Statement(op, lhs, rhs, dest), actual);
        }

    }
}
