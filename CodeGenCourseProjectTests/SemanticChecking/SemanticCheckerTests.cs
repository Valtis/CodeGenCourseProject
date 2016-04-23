using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.SemanticChecking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.Parsing;
using CodeGenCourseProjectTests;

namespace CodeGenCourseProject.SemanticChecking.Tests
{
    [TestClass()]
    public class SemanticCheckerTests
    {

        [TestMethod()]
        public void ValidVariableDeclarationsAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\valid_variable_declarations.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidVariableDeclarationsAreRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\invalid_variable_declarations.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(5, reporter.Errors.Count);

            var helper = new TestHelper(reporter);

            helper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 3, 12, "Redeclaration of identifier 'a'");
            helper.AssertErrorMessage(1, Error.SEMANTIC_ERROR, 7, 12, "Redeclaration of identifier 'a'");
            helper.AssertErrorMessage(2, Error.SEMANTIC_ERROR, 7, 21, "Redeclaration of identifier 'd'");
            helper.AssertErrorMessage(3, Error.SEMANTIC_ERROR, 11, 16, "Type 'integer' is inaccessible");
            helper.AssertErrorMessage(4, Error.SEMANTIC_ERROR, 15, 24, "Type 'integer' is inaccessible");
        }

        [TestMethod()]
        public void ValidExpressionsAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\valid_expressions.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();            

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidExpressionsAreRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\invalid_expressions.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(96, reporter.Errors.Count);

            var helper = new TestHelper(reporter);

            helper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 7, 8, "Identifier 'undeclared' has not");
            helper.AssertErrorMessage(1, Error.SEMANTIC_ERROR, 8, 8, "Identifier 'true' has not");
            helper.AssertErrorMessage(2, Error.SEMANTIC_ERROR, 9, 8, "Identifier 'false' has not");

            helper.AssertErrorMessage(3, Error.SEMANTIC_ERROR, 11, 13, "Cannot assign an expression with type 'string'", "variable with type 'integer'");
            helper.AssertErrorMessage(4, Error.SEMANTIC_ERROR, 12, 13, "Cannot assign an expression with type 'real'", "variable with type 'integer'");
            helper.AssertErrorMessage(5, Error.SEMANTIC_ERROR, 13, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'integer'");
            helper.AssertErrorMessage(6, Error.SEMANTIC_ERROR, 14, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'integer'");

            helper.AssertErrorMessage(7, Error.SEMANTIC_ERROR, 16, 13, "Cannot assign an expression with type 'integer'", "variable with type 'string'");
            helper.AssertErrorMessage(8, Error.SEMANTIC_ERROR, 17, 13, "Cannot assign an expression with type 'real'", "variable with type 'string'");
            helper.AssertErrorMessage(9, Error.SEMANTIC_ERROR, 18, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'string'");
            helper.AssertErrorMessage(10, Error.SEMANTIC_ERROR, 19, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'string'");

            helper.AssertErrorMessage(11, Error.SEMANTIC_ERROR, 21, 13, "Cannot assign an expression with type 'integer'", "variable with type 'real'");
            helper.AssertErrorMessage(12, Error.SEMANTIC_ERROR, 22, 13, "Cannot assign an expression with type 'string'", "variable with type 'real'");
            helper.AssertErrorMessage(13, Error.SEMANTIC_ERROR, 23, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'real'");
            helper.AssertErrorMessage(14, Error.SEMANTIC_ERROR, 24, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'real'");

            helper.AssertErrorMessage(15, Error.SEMANTIC_ERROR, 26, 13, "Cannot assign an expression with type 'integer'", "variable with type 'boolean'");
            helper.AssertErrorMessage(16, Error.SEMANTIC_ERROR, 27, 13, "Cannot assign an expression with type 'string'", "variable with type 'boolean'");
            helper.AssertErrorMessage(17, Error.SEMANTIC_ERROR, 28, 13, "Cannot assign an expression with type 'real'", "variable with type 'boolean'");

            helper.AssertErrorMessage(18, Error.SEMANTIC_ERROR, 30, 21, "Invalid types 'string' and 'integer' for operator '+'");
            helper.AssertErrorMessage(19, Error.SEMANTIC_ERROR, 31, 18, "Invalid types 'boolean' and 'real' for operator '+'");
            helper.AssertErrorMessage(20, Error.SEMANTIC_ERROR, 32, 15, "Invalid types 'integer' and 'string' for operator '+'");
            helper.AssertErrorMessage(21, Error.SEMANTIC_ERROR, 33, 18, "Invalid type 'boolean' for operator '+'");

            helper.AssertErrorMessage(22, Error.SEMANTIC_ERROR, 35, 15, "Invalid types 'integer' and 'real' for operator '-'");
            helper.AssertErrorMessage(23, Error.SEMANTIC_ERROR, 36, 17, "Invalid type 'string' for operator '-'");

            helper.AssertErrorMessage(24, Error.SEMANTIC_ERROR, 38, 15, "Invalid types 'integer' and 'boolean' for operator '*'");
            helper.AssertErrorMessage(25, Error.SEMANTIC_ERROR, 39, 18, "Invalid type 'boolean' for operator '*'");

            helper.AssertErrorMessage(26, Error.SEMANTIC_ERROR, 41, 15, "Invalid types 'integer' and 'real' for operator '/'");
            helper.AssertErrorMessage(27, Error.SEMANTIC_ERROR, 42, 21, "Invalid type 'string' for operator '/'");

            helper.AssertErrorMessage(28, Error.SEMANTIC_ERROR, 44, 15, "Invalid types 'integer' and 'real' for operator '%'");
            helper.AssertErrorMessage(29, Error.SEMANTIC_ERROR, 45, 17, "Invalid type 'real' for operator '%'");

            helper.AssertErrorMessage(30, Error.SEMANTIC_ERROR, 47, 15, "Invalid types 'integer' and 'string' for operator '='");
            helper.AssertErrorMessage(31, Error.SEMANTIC_ERROR, 48, 19, "Invalid types 'boolean' and 'real' for operator '='");
            
            helper.AssertErrorMessage(32, Error.SEMANTIC_ERROR, 50, 15, "Invalid types 'integer' and 'string' for operator '<>'");
            helper.AssertErrorMessage(33, Error.SEMANTIC_ERROR, 51, 18, "Invalid types 'boolean' and 'real' for operator '<>'");

            helper.AssertErrorMessage(34, Error.SEMANTIC_ERROR, 53, 15, "Invalid types 'integer' and 'string' for operator '<'");
            helper.AssertErrorMessage(35, Error.SEMANTIC_ERROR, 54, 21, "Invalid types 'real' and 'boolean' for operator '<'");

            helper.AssertErrorMessage(36, Error.SEMANTIC_ERROR, 56, 15, "Invalid types 'integer' and 'string' for operator '<='");
            helper.AssertErrorMessage(37, Error.SEMANTIC_ERROR, 57, 21, "Invalid types 'real' and 'boolean' for operator '<='");

            helper.AssertErrorMessage(38, Error.SEMANTIC_ERROR, 59, 15, "Invalid types 'integer' and 'string' for operator '>='");
            helper.AssertErrorMessage(39, Error.SEMANTIC_ERROR, 60, 21, "Invalid types 'real' and 'boolean' for operator '>='");

            helper.AssertErrorMessage(40, Error.SEMANTIC_ERROR, 62, 15, "Invalid types 'integer' and 'string' for operator '>'");
            helper.AssertErrorMessage(41, Error.SEMANTIC_ERROR, 63, 21, "Invalid types 'real' and 'boolean' for operator '>'");

            helper.AssertErrorMessage(42, Error.SEMANTIC_ERROR, 65, 15, "Invalid types 'integer' and 'string' for operator 'and'");
            helper.AssertErrorMessage(43, Error.SEMANTIC_ERROR, 66, 18, "Invalid types 'real' and 'boolean' for operator 'and'");
            helper.AssertErrorMessage(44, Error.SEMANTIC_ERROR, 67, 15, "Invalid type 'integer' for operator 'and'");

            helper.AssertErrorMessage(45, Error.SEMANTIC_ERROR, 69, 15, "Invalid types 'integer' and 'string' for operator 'or'");
            helper.AssertErrorMessage(46, Error.SEMANTIC_ERROR, 70, 18, "Invalid types 'real' and 'boolean' for operator 'or'");
            helper.AssertErrorMessage(47, Error.SEMANTIC_ERROR, 71, 15, "Invalid type 'integer' for operator 'or'");

            helper.AssertErrorMessage(48, Error.SEMANTIC_ERROR, 73, 13, "Invalid type 'integer' for operator 'not'");
            helper.AssertErrorMessage(49, Error.SEMANTIC_ERROR, 74, 13, "Invalid type 'string' for operator 'not'");
            helper.AssertErrorMessage(50, Error.SEMANTIC_ERROR, 75, 13, "Invalid type 'real' for operator 'not'");

            helper.AssertErrorMessage(51, Error.SEMANTIC_ERROR, 77, 13, "Invalid type 'string' for operator '-'");
            helper.AssertErrorMessage(52, Error.SEMANTIC_ERROR, 78, 13, "Invalid type 'boolean' for operator '-'");

            helper.AssertErrorMessage(53, Error.SEMANTIC_ERROR, 80, 13, "Invalid type 'string' for operator '+'");
            helper.AssertErrorMessage(54, Error.SEMANTIC_ERROR, 81, 13, "Invalid type 'boolean' for operator '+'");

            helper.AssertErrorMessage(55, Error.SEMANTIC_ERROR, 83, 19, "Cannot assign an expression with type 'real' into a variable with type 'integer'");
            helper.AssertErrorMessage(56, Error.SEMANTIC_ERROR, 84, 21, "Invalid types 'string' and 'integer' for operator '+'");

            helper.AssertErrorMessage(57, Error.SEMANTIC_ERROR, 86, 23, "Invalid type 'string' for array size expression");
            helper.AssertErrorMessage(58, Error.SEMANTIC_ERROR, 87, 28, "Invalid type 'boolean' for array size expression");
            helper.AssertErrorMessage(59, Error.SEMANTIC_ERROR, 88, 23, "Invalid type 'real' for array size expression");
            helper.AssertErrorMessage(60, Error.SEMANTIC_ERROR, 89, 25, "Invalid types 'integer' and 'string' for operator '+'");

            helper.AssertErrorMessage(61, Error.SEMANTIC_ERROR, 96, 19, "Cannot assign an expression with type 'string' into a variable with type 'integer'");
            helper.AssertErrorMessage(62, Error.SEMANTIC_ERROR, 97, 19, "Cannot assign an expression with type 'boolean' into a variable with type 'real'");
            helper.AssertErrorMessage(63, Error.SEMANTIC_ERROR, 98, 18, "Cannot assign an expression with type 'integer' into a variable with type 'string'");
            helper.AssertErrorMessage(64, Error.SEMANTIC_ERROR, 99, 19, "Cannot assign an expression with type 'real' into a variable with type 'boolean'");

            helper.AssertErrorMessage(65, Error.SEMANTIC_ERROR, 101, 8, "Cannot index 'i' as it is not an array");
            helper.AssertErrorMessage(66, Error.SEMANTIC_ERROR, 102, 8, "Cannot index 'r' as it is not an array");
            helper.AssertErrorMessage(67, Error.SEMANTIC_ERROR, 103, 8, "Cannot index 's' as it is not an array");
            helper.AssertErrorMessage(68, Error.SEMANTIC_ERROR, 104, 8, "Cannot index 'b' as it is not an array");
            helper.AssertErrorMessage(69, Error.SEMANTIC_ERROR, 105, 8, "Identifier 'undeclared' has not been declared");

            helper.AssertErrorMessage(70, Error.SEMANTIC_ERROR, 107, 11, "Invalid type 'string' for array indexing");
            helper.AssertErrorMessage(71, Error.SEMANTIC_ERROR, 108, 12, "Invalid type 'real' for array indexing");
            helper.AssertErrorMessage(72, Error.SEMANTIC_ERROR, 109, 11, "Invalid type 'boolean' for array indexing");
            helper.AssertErrorMessage(73, Error.SEMANTIC_ERROR, 110, 17, "Invalid types 'real' and 'integer' for operator '*'");
            helper.AssertErrorMessage(74, Error.SEMANTIC_ERROR, 110, 25, "Cannot assign an expression with type 'string' into a variable with type 'boolean'");

            helper.AssertErrorMessage(75, Error.SEMANTIC_ERROR, 112, 8, "Cannot assign into 'ai' as it is not a regular variable");
            helper.AssertErrorMessage(76, Error.SEMANTIC_ERROR, 113, 8, "Cannot assign into 'as2' as it is not a regular variable");
            helper.AssertErrorMessage(77, Error.SEMANTIC_ERROR, 114, 8, "Cannot assign into 'ab' as it is not a regular variable");
            helper.AssertErrorMessage(78, Error.SEMANTIC_ERROR, 115, 8, "Cannot assign into 'ar2' as it is not a regular variable");

            helper.AssertErrorMessage(79, Error.SEMANTIC_ERROR, 117, 17, "Invalid type 'real' for array indexing");
            helper.AssertErrorMessage(80, Error.SEMANTIC_ERROR, 118, 13, "Cannot assign an expression with type 'string' into a variable with type 'real'");
            helper.AssertErrorMessage(81, Error.SEMANTIC_ERROR, 119, 17, "Invalid type 'string' for array indexing");
            helper.AssertErrorMessage(82, Error.SEMANTIC_ERROR, 119, 26, "Invalid types 'boolean' and 'integer' for operator '+'");
            helper.AssertErrorMessage(83, Error.SEMANTIC_ERROR, 120, 13, "Cannot index 'i' as it is not an array");
            helper.AssertErrorMessage(84, Error.SEMANTIC_ERROR, 121, 13, "Identifier 'undeclared' has not been declared");

            helper.AssertErrorMessage(85, Error.SEMANTIC_ERROR, 123, 8, "Identifier 'true' has not been declared");
            helper.AssertErrorMessage(86, Error.SEMANTIC_ERROR, 124, 8, "Identifier 'false' has not been declared");
            
            helper.AssertErrorMessage(87, Error.SEMANTIC_ERROR, 129, 13, "Cannot assign an expression with type 'integer' into a variable with type 'boolean'");
            helper.AssertErrorMessage(88, Error.SEMANTIC_ERROR, 130, 13, "Cannot assign an expression with type 'integer' into a variable with type 'boolean'");
            helper.AssertErrorMessage(89, Error.SEMANTIC_ERROR, 131, 18, "Invalid type 'integer' for operator 'and'");

            helper.AssertErrorMessage(90, Error.SEMANTIC_ERROR, 133, 22, "Invalid types 'string' and 'real' for operator '*'");
            helper.AssertErrorMessage(91, Error.SEMANTIC_ERROR, 134, 23, "Identifier 'undeclared' has not been declared");

            helper.AssertErrorMessage(92, Error.SEMANTIC_ERROR, 138, 21, "Cannot assign an expression with type 'integer' into a variable with type 'boolean'");

            helper.AssertErrorMessage(93, Error.SEMANTIC_ERROR, 141, 17, "Invalid types 'real' and 'integer' for operator '+'");
            helper.AssertErrorMessage(94, Error.SEMANTIC_ERROR, 142, 13, "Cannot get the size of an non-array object 'i'");
            helper.AssertErrorMessage(95, Error.SEMANTIC_ERROR, 143, 13, "Identifier 'undeclared' has not been declared");

        }
    }
}