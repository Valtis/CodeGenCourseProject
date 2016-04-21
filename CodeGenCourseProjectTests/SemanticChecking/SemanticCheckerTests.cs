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

            Assert.AreEqual(42, reporter.Errors.Count);

            var helper = new TestHelper(reporter);

            helper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 7, 8, "Variable 'undeclared' has not");
            helper.AssertErrorMessage(1, Error.SEMANTIC_ERROR, 8, 8, "Variable 'true' has not");
            helper.AssertErrorMessage(2, Error.SEMANTIC_ERROR, 9, 8, "Variable 'false' has not");

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
        }
    }
}