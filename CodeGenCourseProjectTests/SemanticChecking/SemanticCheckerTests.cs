using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var reporter = new MessageReporter();
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
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\invalid_variable_declarations.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(5, reporter.Errors.Count);

            var helper = new TestHelper(reporter);

            helper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 3, 12, "Redeclaration of identifier 'a'");
            helper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 7, 12, "Redeclaration of identifier 'a'");
            helper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 7, 21, "Redeclaration of identifier 'd'");
            helper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 11, 16, "Type 'integer' is inaccessible");
            helper.AssertErrorMessage(4, MessageKind.SEMANTIC_ERROR, 15, 24, "Type 'integer' is inaccessible");
        }

        [TestMethod()]
        public void ValidWhileStatementsAreAccepted()
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\valid_while_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidWhileStatementsAreRejected()
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\invalid_while_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(8, reporter.Errors.Count);

            var helper = new TestHelper(reporter);
            helper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 3, 14, "Invalid type 'integer' for while "); 
            helper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 8, 14, "Invalid type 'string' for while ");
            helper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 12, 14, "Invalid type 'real' for while ");
            helper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 20, 14, "Invalid type 'string' for while ");
            helper.AssertErrorMessage(4, MessageKind.SEMANTIC_ERROR, 25, 14, "Invalid type 'integer' for while ");
            helper.AssertErrorMessage(5, MessageKind.SEMANTIC_ERROR, 30, 14, "Identifier 'undeclared' has not");
            helper.AssertErrorMessage(6, MessageKind.SEMANTIC_ERROR, 38, 21, "Cannot assign an expression with type 'integer' into a variable with type 'string'");
            helper.AssertErrorMessage(7, MessageKind.SEMANTIC_ERROR, 41, 16, "Invalid types 'integer' and 'string' for operator '<'");
        }

        [TestMethod()]
        public void ValidAssertStatementsAreAccepted()
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\valid_assert_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidAssertStatementsAreRejected()
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\invalid_assert_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(4, reporter.Errors.Count);
            var helper = new TestHelper(reporter);
            helper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 2, 15, "Invalid type 'integer' for assert");
            helper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 3, 15, "Invalid type 'string' for assert");
            helper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 4, 15, "Invalid type 'real' for assert");
            helper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 5, 15, "Invalid type 'string' for operator 'not'");
        }

        [TestMethod()]
        public void ValidIfExpressionsAreAccepted()
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\valid_if_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidIfExpressionsAreRejected()
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\invalid_if_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(6, reporter.Errors.Count);
            var helper = new TestHelper(reporter);
            helper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 2, 11, "Invalid type 'integer' for if");
            helper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 5, 11, "Invalid type 'real' for if");
            helper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 8, 11, "Invalid type 'string' for if");
            helper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 11, 13, "Invalid types 'integer' and 'boolean' for operator '<'");
            helper.AssertErrorMessage(4, MessageKind.SEMANTIC_ERROR, 17, 21, "Cannot assign an expression with type 'boolean' into a variable with type 'integer'");
            helper.AssertErrorMessage(5, MessageKind.SEMANTIC_ERROR, 22, 21, "Cannot assign an expression with type 'integer' into a variable with type 'Array<integer>'");
        }

        [TestMethod()]
        public void ValidProceduresAndFunctionsAreAccepted()
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\valid_procedures_and_functions.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidProceduresAndFunctionsAreRejected()
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\invalid_procedures_and_functions.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(50, reporter.Errors.Count);

            var helper = new TestHelper(reporter);

            helper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 5, 21, "Cannot assign an expression with type 'string' into a variable with type 'integer'");
            helper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 10, 20, "Redeclaration of identifier 'a'");
            helper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 10, 23, "Redeclaration of identifier 'b'");
            helper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 13, 54, "Invalid type 'string' for array size expression");
            helper.AssertErrorMessage(4, MessageKind.SEMANTIC_ERROR, 19, 39, "Type 'real' is inaccessible");
            helper.AssertErrorMessage(5, MessageKind.SEMANTIC_ERROR, 19, 61, "Type 'real' is inaccessible");
            helper.AssertErrorMessage(6, MessageKind.SEMANTIC_ERROR, 24, 59, "Redeclaration of identifier 'a'");
            helper.AssertErrorMessage(7, MessageKind.SEMANTIC_ERROR, 24, 76, "Redeclaration of identifier 'a'");
            helper.AssertErrorMessage(8, MessageKind.SEMANTIC_ERROR, 35, 18, "Redeclaration of identifier 'proc'");
            helper.AssertErrorMessage(9, MessageKind.SEMANTIC_ERROR, 40, 12, "Redeclaration of identifier 'proc'");
            helper.AssertErrorMessage(10, MessageKind.SEMANTIC_ERROR, 43, 18, "Redeclaration of identifier 'variable'");
            helper.AssertErrorMessage(11, MessageKind.SEMANTIC_ERROR, 53, 8, "Identifier 'undeclared' has not been declared");
            helper.AssertErrorMessage(12, MessageKind.SEMANTIC_ERROR, 54, 8, "Identifier 'variable' is not callable");
            helper.AssertErrorMessage(13, MessageKind.SEMANTIC_ERROR, 56, 8, "Call to 'proc' has 2 arguments but 'proc' has 0 parameters");
            helper.AssertErrorMessage(14, MessageKind.SEMANTIC_ERROR, 57, 14, "Argument 1 for 'proc2' has type 'real' but corresponding parameter has type 'integer'");
            helper.AssertErrorMessage(15, MessageKind.SEMANTIC_ERROR, 57, 21, "Argument 2 for 'proc2' has type 'boolean' but corresponding parameter has type 'Array<string>'");
            helper.AssertErrorMessage(16, MessageKind.SEMANTIC_ERROR, 59, 8, "Cannot assign into function or procedure"); helper.AssertErrorMessage(17, MessageKind.SEMANTIC_ERROR, 60, 8, "Cannot index 'proc' as it is not an array");
            helper.AssertErrorMessage(17, MessageKind.SEMANTIC_ERROR, 60, 8, "Cannot index 'proc' as it is not an array");
            helper.AssertErrorMessage(18, MessageKind.SEMANTIC_ERROR, 60, 8, "Cannot assign into function or procedure");
            helper.AssertErrorMessage(19, MessageKind.SEMANTIC_ERROR, 63, 18, "Invalid types 'Function<void>' and 'integer' for operator '+'");
            helper.AssertErrorMessage(20, MessageKind.SEMANTIC_ERROR, 64, 20, "Invalid types 'void' and 'integer' for operator '+'");
            helper.AssertErrorMessage(21, MessageKind.SEMANTIC_ERROR, 67, 8, "Identifier 'integer' has not been declared");
            helper.AssertErrorMessage(22, MessageKind.SEMANTIC_ERROR, 68, 8, "Predefined procedure 'read' expects at least one argument");
            helper.AssertErrorMessage(23, MessageKind.SEMANTIC_ERROR, 69, 13, "Identifier 'ff' has not been declared");
            helper.AssertErrorMessage(24, MessageKind.SEMANTIC_ERROR, 70, 15, "Invalid argument for predefined function 'read'");
            helper.AssertErrorMessage(25, MessageKind.SEMANTIC_ERROR, 71, 13, "Invalid argument for predefined function 'read'");
            helper.AssertErrorMessage(26, MessageKind.SEMANTIC_ERROR, 72, 13, "Invalid argument for predefined function 'read'");
            helper.AssertErrorMessage(27, MessageKind.SEMANTIC_ERROR, 74, 13, "Invalid argument for predefined function 'read'");
            helper.AssertErrorMessage(28, MessageKind.SEMANTIC_ERROR, 76, 16, "Invalid argument type 'Function<void>' for predefined function 'writeln'");
            helper.AssertErrorMessage(29, MessageKind.SEMANTIC_ERROR, 76, 22, "Invalid argument type 'void' for predefined function 'writeln'");
            helper.AssertErrorMessage(30, MessageKind.SEMANTIC_ERROR, 77, 18, "Invalid types 'integer' and 'string' for operator '='");
            helper.AssertErrorMessage(31, MessageKind.SEMANTIC_ERROR, 79, 15, "Cannot assign an expression with type 'Function<void>' into a variable with type 'Array<integer>'");
            helper.AssertErrorMessage(32, MessageKind.SEMANTIC_ERROR, 83, 16, "Return statement in procedure cannot return a value");
            helper.AssertErrorMessage(33, MessageKind.SEMANTIC_ERROR, 84, 23, "Identifier 'barf' has not been declared");
            helper.AssertErrorMessage(34, MessageKind.SEMANTIC_ERROR, 87, 8, "Return statement outside function or procedure body");
            helper.AssertErrorMessage(35, MessageKind.SEMANTIC_ERROR, 93, 32, "Invalid types 'integer' and 'string' for operator '*'");
            helper.AssertErrorMessage(36, MessageKind.SEMANTIC_ERROR, 95, 16, "Return statement in procedure cannot return a value");
            helper.AssertErrorMessage(37, MessageKind.SEMANTIC_ERROR, 98, 44, "Type 'real' is inaccessible");
            helper.AssertErrorMessage(38, MessageKind.SEMANTIC_ERROR, 98, 70, "Type 'real' is inaccessible");
            helper.AssertErrorMessage(39, MessageKind.SEMANTIC_ERROR, 105, 23, "Return statement has type 'real' when enclosing function has type 'integer'");
            helper.AssertErrorMessage(40, MessageKind.SEMANTIC_ERROR, 110, 16, "Return statement must have an expression with type 'integer', as it is enclosed by a function, not procedure");
            helper.AssertErrorMessage(41, MessageKind.SEMANTIC_ERROR, 115, 23, "Return statement has type 'integer' when enclosing function has type 'Array<integer>'");
            helper.AssertErrorMessage(42, MessageKind.SEMANTIC_ERROR, 118, 60, "Redeclaration of identifier 'a'");
            helper.AssertErrorMessage(43, MessageKind.SEMANTIC_ERROR, 123, 76, "Type 'real' is inaccessible");
            helper.AssertErrorMessage(44, MessageKind.SEMANTIC_ERROR, 135, 16, "Identifier 'valid' is not callable");
            helper.AssertErrorMessage(45, MessageKind.SEMANTIC_ERROR, 139, 23, "Invalid types 'string' and 'integer' for operator '+'");
            helper.AssertErrorMessage(46, MessageKind.SEMANTIC_ERROR, 147, 8, "Call to 'valid' has 1 arguments but 'valid' has 0 parameters");
            helper.AssertErrorMessage(47, MessageKind.SEMANTIC_ERROR, 148, 15, "Argument 1 for 'valid2' has type 'integer' but corresponding parameter has type 'string'");
            helper.AssertErrorMessage(48, MessageKind.SEMANTIC_ERROR, 152, 16, "Call to 'foo' has 2 arguments but 'foo' has 1 parameters");
            helper.AssertErrorMessage(49, MessageKind.SEMANTIC_ERROR, 156, 13, "Invalid argument for predefined function 'read'");
        }
        

        [TestMethod()]
        public void ValidExpressionsAreAccepted()
        {
            var reporter = new MessageReporter();
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
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\invalid_expressions.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);

            Assert.AreEqual(116, reporter.Errors.Count);

            var helper = new TestHelper(reporter);

            helper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 7, 8, "Identifier 'undeclared' has not");
            helper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 8, 8, "Identifier 'true' has not");
            helper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 9, 8, "Identifier 'false' has not");

            helper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 11, 13, "Cannot assign an expression with type 'string'", "variable with type 'integer'");
            helper.AssertErrorMessage(4, MessageKind.SEMANTIC_ERROR, 12, 13, "Cannot assign an expression with type 'real'", "variable with type 'integer'");
            helper.AssertErrorMessage(5, MessageKind.SEMANTIC_ERROR, 13, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'integer'");
            helper.AssertErrorMessage(6, MessageKind.SEMANTIC_ERROR, 14, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'integer'");

            helper.AssertErrorMessage(7, MessageKind.SEMANTIC_ERROR, 16, 13, "Cannot assign an expression with type 'integer'", "variable with type 'string'");
            helper.AssertErrorMessage(8, MessageKind.SEMANTIC_ERROR, 17, 13, "Cannot assign an expression with type 'real'", "variable with type 'string'");
            helper.AssertErrorMessage(9, MessageKind.SEMANTIC_ERROR, 18, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'string'");
            helper.AssertErrorMessage(10, MessageKind.SEMANTIC_ERROR, 19, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'string'");

            helper.AssertErrorMessage(11, MessageKind.SEMANTIC_ERROR, 21, 13, "Cannot assign an expression with type 'integer'", "variable with type 'real'");
            helper.AssertErrorMessage(12, MessageKind.SEMANTIC_ERROR, 22, 13, "Cannot assign an expression with type 'string'", "variable with type 'real'");
            helper.AssertErrorMessage(13, MessageKind.SEMANTIC_ERROR, 23, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'real'");
            helper.AssertErrorMessage(14, MessageKind.SEMANTIC_ERROR, 24, 13, "Cannot assign an expression with type 'boolean'", "variable with type 'real'");

            helper.AssertErrorMessage(15, MessageKind.SEMANTIC_ERROR, 26, 13, "Cannot assign an expression with type 'integer'", "variable with type 'boolean'");
            helper.AssertErrorMessage(16, MessageKind.SEMANTIC_ERROR, 27, 13, "Cannot assign an expression with type 'string'", "variable with type 'boolean'");
            helper.AssertErrorMessage(17, MessageKind.SEMANTIC_ERROR, 28, 13, "Cannot assign an expression with type 'real'", "variable with type 'boolean'");

            helper.AssertErrorMessage(18, MessageKind.SEMANTIC_ERROR, 30, 21, "Invalid types 'string' and 'integer' for operator '+'");
            helper.AssertErrorMessage(19, MessageKind.SEMANTIC_ERROR, 31, 18, "Invalid types 'boolean' and 'real' for operator '+'");
            helper.AssertErrorMessage(20, MessageKind.SEMANTIC_ERROR, 32, 15, "Invalid types 'integer' and 'string' for operator '+'");
            helper.AssertErrorMessage(21, MessageKind.SEMANTIC_ERROR, 33, 18, "Invalid type 'boolean' for operator '+'");

            helper.AssertErrorMessage(22, MessageKind.SEMANTIC_ERROR, 35, 15, "Invalid types 'integer' and 'real' for operator '-'");
            helper.AssertErrorMessage(23, MessageKind.SEMANTIC_ERROR, 36, 17, "Invalid type 'string' for operator '-'");

            helper.AssertErrorMessage(24, MessageKind.SEMANTIC_ERROR, 38, 15, "Invalid types 'integer' and 'boolean' for operator '*'");
            helper.AssertErrorMessage(25, MessageKind.SEMANTIC_ERROR, 39, 18, "Invalid type 'boolean' for operator '*'");

            helper.AssertErrorMessage(26, MessageKind.SEMANTIC_ERROR, 41, 15, "Invalid types 'integer' and 'real' for operator '/'");
            helper.AssertErrorMessage(27, MessageKind.SEMANTIC_ERROR, 42, 21, "Invalid type 'string' for operator '/'");

            helper.AssertErrorMessage(28, MessageKind.SEMANTIC_ERROR, 44, 15, "Invalid types 'integer' and 'real' for operator '%'");
            helper.AssertErrorMessage(29, MessageKind.SEMANTIC_ERROR, 45, 17, "Invalid type 'real' for operator '%'");

            helper.AssertErrorMessage(30, MessageKind.SEMANTIC_ERROR, 47, 15, "Invalid types 'integer' and 'string' for operator '='");
            helper.AssertErrorMessage(31, MessageKind.SEMANTIC_ERROR, 48, 19, "Invalid types 'boolean' and 'real' for operator '='");
            
            helper.AssertErrorMessage(32, MessageKind.SEMANTIC_ERROR, 50, 15, "Invalid types 'integer' and 'string' for operator '<>'");
            helper.AssertErrorMessage(33, MessageKind.SEMANTIC_ERROR, 51, 18, "Invalid types 'boolean' and 'real' for operator '<>'");

            helper.AssertErrorMessage(34, MessageKind.SEMANTIC_ERROR, 53, 15, "Invalid types 'integer' and 'string' for operator '<'");
            helper.AssertErrorMessage(35, MessageKind.SEMANTIC_ERROR, 54, 21, "Invalid types 'real' and 'boolean' for operator '<'");

            helper.AssertErrorMessage(36, MessageKind.SEMANTIC_ERROR, 56, 15, "Invalid types 'integer' and 'string' for operator '<='");
            helper.AssertErrorMessage(37, MessageKind.SEMANTIC_ERROR, 57, 21, "Invalid types 'real' and 'boolean' for operator '<='");

            helper.AssertErrorMessage(38, MessageKind.SEMANTIC_ERROR, 59, 15, "Invalid types 'integer' and 'string' for operator '>='");
            helper.AssertErrorMessage(39, MessageKind.SEMANTIC_ERROR, 60, 21, "Invalid types 'real' and 'boolean' for operator '>='");

            helper.AssertErrorMessage(40, MessageKind.SEMANTIC_ERROR, 62, 15, "Invalid types 'integer' and 'string' for operator '>'");
            helper.AssertErrorMessage(41, MessageKind.SEMANTIC_ERROR, 63, 21, "Invalid types 'real' and 'boolean' for operator '>'");

            helper.AssertErrorMessage(42, MessageKind.SEMANTIC_ERROR, 65, 15, "Invalid types 'integer' and 'string' for operator 'and'");
            helper.AssertErrorMessage(43, MessageKind.SEMANTIC_ERROR, 66, 18, "Invalid types 'real' and 'boolean' for operator 'and'");
            helper.AssertErrorMessage(44, MessageKind.SEMANTIC_ERROR, 67, 15, "Invalid type 'integer' for operator 'and'");

            helper.AssertErrorMessage(45, MessageKind.SEMANTIC_ERROR, 69, 15, "Invalid types 'integer' and 'string' for operator 'or'");
            helper.AssertErrorMessage(46, MessageKind.SEMANTIC_ERROR, 70, 18, "Invalid types 'real' and 'boolean' for operator 'or'");
            helper.AssertErrorMessage(47, MessageKind.SEMANTIC_ERROR, 71, 15, "Invalid type 'integer' for operator 'or'");

            helper.AssertErrorMessage(48, MessageKind.SEMANTIC_ERROR, 73, 13, "Invalid type 'integer' for operator 'not'");
            helper.AssertErrorMessage(49, MessageKind.SEMANTIC_ERROR, 74, 13, "Invalid type 'string' for operator 'not'");
            helper.AssertErrorMessage(50, MessageKind.SEMANTIC_ERROR, 75, 13, "Invalid type 'real' for operator 'not'");

            helper.AssertErrorMessage(51, MessageKind.SEMANTIC_ERROR, 77, 13, "Invalid type 'string' for operator '-'");
            helper.AssertErrorMessage(52, MessageKind.SEMANTIC_ERROR, 78, 13, "Invalid type 'boolean' for operator '-'");

            helper.AssertErrorMessage(53, MessageKind.SEMANTIC_ERROR, 80, 13, "Invalid type 'string' for operator '+'");
            helper.AssertErrorMessage(54, MessageKind.SEMANTIC_ERROR, 81, 13, "Invalid type 'boolean' for operator '+'");

            helper.AssertErrorMessage(55, MessageKind.SEMANTIC_ERROR, 83, 19, "Cannot assign an expression with type 'real' into a variable with type 'integer'");
            helper.AssertErrorMessage(56, MessageKind.SEMANTIC_ERROR, 84, 21, "Invalid types 'string' and 'integer' for operator '+'");

            helper.AssertErrorMessage(57, MessageKind.SEMANTIC_ERROR, 86, 23, "Invalid type 'string' for array size expression");
            helper.AssertErrorMessage(58, MessageKind.SEMANTIC_ERROR, 87, 28, "Invalid type 'boolean' for array size expression");
            helper.AssertErrorMessage(59, MessageKind.SEMANTIC_ERROR, 88, 23, "Invalid type 'real' for array size expression");
            helper.AssertErrorMessage(60, MessageKind.SEMANTIC_ERROR, 89, 25, "Invalid types 'integer' and 'string' for operator '+'");

            helper.AssertErrorMessage(61, MessageKind.SEMANTIC_ERROR, 96, 19, "Cannot assign an expression with type 'string' into a variable with type 'integer'");
            helper.AssertErrorMessage(62, MessageKind.SEMANTIC_ERROR, 97, 19, "Cannot assign an expression with type 'boolean' into a variable with type 'real'");
            helper.AssertErrorMessage(63, MessageKind.SEMANTIC_ERROR, 98, 18, "Cannot assign an expression with type 'integer' into a variable with type 'string'");
            helper.AssertErrorMessage(64, MessageKind.SEMANTIC_ERROR, 99, 19, "Cannot assign an expression with type 'real' into a variable with type 'boolean'");

            helper.AssertErrorMessage(65, MessageKind.SEMANTIC_ERROR, 101, 8, "Cannot index 'i' as it is not an array");
            helper.AssertErrorMessage(66, MessageKind.SEMANTIC_ERROR, 102, 8, "Cannot index 'r' as it is not an array");
            helper.AssertErrorMessage(67, MessageKind.SEMANTIC_ERROR, 103, 8, "Cannot index 's' as it is not an array");
            helper.AssertErrorMessage(68, MessageKind.SEMANTIC_ERROR, 104, 8, "Cannot index 'b' as it is not an array");
            helper.AssertErrorMessage(69, MessageKind.SEMANTIC_ERROR, 105, 8, "Identifier 'undeclared' has not been declared");

            helper.AssertErrorMessage(70, MessageKind.SEMANTIC_ERROR, 107, 11, "Invalid type 'string' for array indexing");
            helper.AssertErrorMessage(71, MessageKind.SEMANTIC_ERROR, 108, 12, "Invalid type 'real' for array indexing");
            helper.AssertErrorMessage(72, MessageKind.SEMANTIC_ERROR, 109, 11, "Invalid type 'boolean' for array indexing");
            helper.AssertErrorMessage(73, MessageKind.SEMANTIC_ERROR, 110, 17, "Invalid types 'real' and 'integer' for operator '*'");
            helper.AssertErrorMessage(74, MessageKind.SEMANTIC_ERROR, 110, 25, "Cannot assign an expression with type 'string' into a variable with type 'boolean'");

            helper.AssertErrorMessage(75, MessageKind.SEMANTIC_ERROR, 112, 14, "Cannot assign an expression with type 'integer' into a variable with type 'Array<integer>'");
            helper.AssertErrorMessage(76, MessageKind.SEMANTIC_ERROR, 113, 15, "Cannot assign an expression with type 'string' into a variable with type 'Array<string>'");
            helper.AssertErrorMessage(77, MessageKind.SEMANTIC_ERROR, 114, 14, "Cannot assign an expression with type 'integer' into a variable with type 'Array<boolean>'");
            helper.AssertErrorMessage(78, MessageKind.SEMANTIC_ERROR, 115, 15, "Cannot assign an expression with type 'boolean' into a variable with type 'Array<real>'");

            helper.AssertErrorMessage(79, MessageKind.SEMANTIC_ERROR, 117, 17, "Invalid type 'real' for array indexing");
            helper.AssertErrorMessage(80, MessageKind.SEMANTIC_ERROR, 118, 13, "Cannot assign an expression with type 'string' into a variable with type 'real'");
            helper.AssertErrorMessage(81, MessageKind.SEMANTIC_ERROR, 119, 17, "Invalid type 'string' for array indexing");
            helper.AssertErrorMessage(82, MessageKind.SEMANTIC_ERROR, 119, 26, "Invalid types 'boolean' and 'integer' for operator '+'");
            helper.AssertErrorMessage(83, MessageKind.SEMANTIC_ERROR, 120, 13, "Cannot index 'i' as it is not an array");
            helper.AssertErrorMessage(84, MessageKind.SEMANTIC_ERROR, 121, 13, "Identifier 'undeclared' has not been declared");

            helper.AssertErrorMessage(85, MessageKind.SEMANTIC_ERROR, 123, 8, "Identifier 'true' has not been declared");
            helper.AssertErrorMessage(86, MessageKind.SEMANTIC_ERROR, 124, 8, "Identifier 'false' has not been declared");
            
            helper.AssertErrorMessage(87, MessageKind.SEMANTIC_ERROR, 129, 13, "Cannot assign an expression with type 'integer' into a variable with type 'boolean'");
            helper.AssertErrorMessage(88, MessageKind.SEMANTIC_ERROR, 130, 13, "Cannot assign an expression with type 'integer' into a variable with type 'boolean'");
            helper.AssertErrorMessage(89, MessageKind.SEMANTIC_ERROR, 131, 18, "Invalid type 'integer' for operator 'and'");

            helper.AssertErrorMessage(90, MessageKind.SEMANTIC_ERROR, 133, 22, "Invalid types 'string' and 'real' for operator '*'");
            helper.AssertErrorMessage(91, MessageKind.SEMANTIC_ERROR, 134, 23, "Identifier 'undeclared' has not been declared");

            helper.AssertErrorMessage(92, MessageKind.SEMANTIC_ERROR, 138, 21, "Cannot assign an expression with type 'integer' into a variable with type 'boolean'");

            helper.AssertErrorMessage(93, MessageKind.SEMANTIC_ERROR, 141, 17, "Invalid types 'real' and 'integer' for operator '+'");
            helper.AssertErrorMessage(94, MessageKind.SEMANTIC_ERROR, 142, 13, "Cannot get the size of an expression with type 'integer'");
            helper.AssertErrorMessage(95, MessageKind.SEMANTIC_ERROR, 143, 13, "Identifier 'undeclared' has not been declared");

            helper.AssertErrorMessage(96, MessageKind.SEMANTIC_ERROR, 146, 29, "Cannot assign an expression with type 'Array<integer>' into a variable with type 'Array<string>'");
            helper.AssertErrorMessage(97, MessageKind.SEMANTIC_ERROR, 148, 36, "Type 'integer' is inaccessible");
            helper.AssertErrorMessage(98, MessageKind.SEMANTIC_ERROR, 150, 17, "Invalid types 'Array<integer>' and 'integer' for operator '+'");
            helper.AssertErrorMessage(99, MessageKind.SEMANTIC_ERROR, 151, 13, "Invalid type 'Array<boolean>' for operator 'not'");
            helper.AssertErrorMessage(100, MessageKind.SEMANTIC_ERROR, 152, 13, "Cannot assign an expression with type 'Array<boolean>' into ");
            helper.AssertErrorMessage(101, MessageKind.SEMANTIC_ERROR, 153, 17, "Invalid type 'Array<integer>' for operator '+'");

            helper.AssertErrorMessage(102, MessageKind.SEMANTIC_ERROR, 155, 8, "Identifier 'boolean' has not been declared");
            helper.AssertErrorMessage(103, MessageKind.SEMANTIC_ERROR, 156, 8, "Identifier 'writeln' has not been declared");
            helper.AssertErrorMessage(104, MessageKind.SEMANTIC_ERROR, 157, 8, "Identifier 'read' has not been declared");
            helper.AssertErrorMessage(105, MessageKind.SEMANTIC_ERROR, 158, 15, "Invalid types 'integer' and '<predefined identifier>' for operator '+'");
            helper.AssertErrorMessage(106, MessageKind.SEMANTIC_ERROR, 159, 15, "Invalid types 'integer' and '<predefined identifier>' for operator '+'");
            helper.AssertErrorMessage(107, MessageKind.SEMANTIC_ERROR, 160, 13, "Cannot assign an expression with type '<predefined identifier>' into a variable with type 'integer'");
            helper.AssertErrorMessage(108, MessageKind.SEMANTIC_ERROR, 161, 13, "Cannot assign an expression with type '<predefined identifier>' into a variable with type 'integer'");
            helper.AssertErrorMessage(109, MessageKind.SEMANTIC_ERROR, 162, 13, "Invalid type '<predefined identifier>' for operator '-'");
            helper.AssertErrorMessage(110, MessageKind.SEMANTIC_ERROR, 163, 13, "Invalid type '<predefined identifier>' for operator '-'");
            helper.AssertErrorMessage(111, MessageKind.SEMANTIC_ERROR, 164, 8, "Identifier 'read'");

            helper.AssertErrorMessage(112, MessageKind.SEMANTIC_ERROR, 166, 14, "Cannot get the size of an expression with type 'integer'");
            helper.AssertErrorMessage(113, MessageKind.SEMANTIC_ERROR, 167, 14, "Cannot get the size of an expression with type 'string'");
            helper.AssertErrorMessage(114, MessageKind.SEMANTIC_ERROR, 168, 14, "Cannot get the size of an expression with type 'boolean'");
            helper.AssertErrorMessage(115, MessageKind.SEMANTIC_ERROR, 169, 16, "Cannot get the size of an expression with type 'boolean'");
        }

        [TestMethod()]
        public void InvalidReferenceArgumentsAreRejected()
        {
            var reporter = new MessageReporter();
            var lexer = new Lexer(@"..\..\SemanticChecking\invalid_ref_args.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);
            Assert.AreEqual(4, reporter.Errors.Count);

            var helper = new TestHelper(reporter);
            helper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 22, 16, "Argument 1 for 'refargs' is invalid, as the corresponding parameter is reference type");
            helper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 22, 23, "Argument 2 for 'refargs' is invalid, as the corresponding parameter is reference type");
            helper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 22, 29, "Argument 3 for 'refargs' is invalid, as the corresponding parameter is reference type");
            helper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 22, 38, "Argument 4 for 'refargs' is invalid, as the corresponding parameter is reference type");

        }

    }
}
