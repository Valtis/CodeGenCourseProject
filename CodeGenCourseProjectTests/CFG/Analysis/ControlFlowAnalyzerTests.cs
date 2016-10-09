using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.Parsing;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC;
using CodeGenCourseProjectTests;

namespace CodeGenCourseProject.CFG.Analysis.Tests
{
    [TestClass()]
    public class ControlFlowAnalyzerTests
    {
        private void Analyze(string name, MessageReporter reporter)
        {
            var lexer = new Lexer(@"..\..\CFG\Analysis\" + name, reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);
            Assert.AreEqual(0, reporter.Errors.Count);

            var tacGenerator = new Generator();
            node.Accept(tacGenerator);

            var cfgGenerator = new CFGGenerator(tacGenerator.Functions);

            var analyzer = new CFGAnalyzer(
                reporter,
                tacGenerator.Functions,
                cfgGenerator.GenerateCFG());
            analyzer.Analyze();
        }

        [TestMethod()]
        public void UninitializedVariableIsErrorWhenNoBranching()
        {
            var reporter = new MessageReporter();
            Analyze("uninitialized_variable_no_branches.txt", reporter);

            Assert.AreEqual(1, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 4, 13, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void SelfAssignmentIsError()
        {
            var reporter = new MessageReporter();
            Analyze("self_assignment.txt", reporter);

            Assert.AreEqual(1, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 3, 8, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void UsingVariableBeforeInitializationInSameBasicBlockIsError()
        {
            var reporter = new MessageReporter();
            Analyze("usage_before_assignment.txt", reporter);

            Assert.AreEqual(1, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 3, 13, "Usage of uninitialized variable 'a'");
        }
        
        [TestMethod()]
        public void InitalizedVariablesCauseNoErrorsWhenNoBranching()
        {
            var reporter = new MessageReporter();
            Analyze("initialized_variable_no_branches.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);

        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization()
        {
            var reporter = new MessageReporter();
            Analyze("valid_if_else_initialization.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);

        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization2()
        {
            var reporter = new MessageReporter();
            Analyze("valid_if_branch_only_initialization.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization3()
        {
            var reporter = new MessageReporter();
            Analyze("valid_else_branch_only_initialization.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization4()
        {
            var reporter = new MessageReporter();
            Analyze("valid_if_branch_initialization_with_dead_else.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization5()
        {
            var reporter = new MessageReporter();
            Analyze("valid_nested_if_statements.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidIfStatementVariableInitialization()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_if_branch_only_initialization.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 9, 17, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void InvalidIfStatementVariableInitialization2()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_if_branch_only_initialization_with_else_branch.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 12, 13, "Usage of uninitialized variable 'b'");
        }

        [TestMethod()]
        public void InvalidIfStatementVariableInitialization3()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_nested_if_statements.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 24, 13, "Usage of uninitialized variable 'c'");
        }

        [TestMethod()]
        public void InvalidIfStatementVariableInitialization4()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_variable_used_in_if_blocks_before_initialization_in_block.txt", reporter);
            Assert.AreEqual(2, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 6, 21, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 11, 21, "Usage of uninitialized variable 'b'");
        }
        
        [TestMethod()]
        public void UsingUninitalizedVariableInIfElseBlocksIsError()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_variable_if_block.txt", reporter);
            Assert.AreEqual(2, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 7, 21, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 9, 25, "Usage of uninitialized variable 'a'");
        }
        
        [TestMethod()]
        public void UninitializedVariableIsErrorInIfCondition()
        {
            var reporter = new MessageReporter();
            Analyze("uninitialized_variable_in_if_condition.txt", reporter);
            Assert.AreEqual(2, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 4, 11, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 4, 19, "Usage of uninitialized variable 'b'");
        }


        [TestMethod()]
        public void ValidWhileStatementVariableUsage()
        {
            var reporter = new MessageReporter();
            Analyze("valid_while_statement.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
        }

        [TestMethod()]
        public void ValidWhileStatement2()
        {
            var reporter = new MessageReporter();
            Analyze("valid_while_statement2.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
        }

        [TestMethod()]
        public void ValidWhileStatement3()
        {
            var reporter = new MessageReporter();
            Analyze("valid_while_statement3.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
        }

        [TestMethod()]
        public void InvalidWhileStatement1()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_while_statement1.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 4, 14, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void InvalidWhileStatement2()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_while_statement2.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 6, 21, "Usage of uninitialized variable 'b'");
        }

        [TestMethod()]
        public void InvalidWhileStatement3()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_while_statement3.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 7, 13, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void InvalidWhileStatement4()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_while_statement4.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 6, 30, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void InvalidWhileStatement5()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_while_statement5.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 28, 13, "Usage of uninitialized variable 'c'");
        }

        [TestMethod()]
        public void ValidArrays()
        {
            var reporter = new MessageReporter();
            Analyze("valid_arrays.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidProcedure()
        {
            var reporter = new MessageReporter();
            Analyze("valid_variables_in_procedures.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidProcedure1()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_procedure1.txt", reporter);
            Assert.AreEqual(2, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 5, 19, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 10, 21, "Usage of uninitialized variable 'b'");
        }

        [TestMethod()]
        public void ValidFunction()
        {
            var reporter = new MessageReporter();
            Analyze("valid_functions.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidFunction()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_functions.txt", reporter);
            Assert.AreEqual(3, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 2, 8, "Not all control paths return a value in function 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 32, 29, "Usage of uninitialized variable 'c'");
            testHelper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 19, 8, "Not all control paths return a value in function 'b'");
        }

        [TestMethod()]
        public void ValidVariableCapture()
        {
            var reporter = new MessageReporter();
            Analyze("valid_function_variable_capture.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);            
        }

        [TestMethod()]
        public void InvalidVariableCapture()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_variable_capture.txt", reporter);
            Assert.AreEqual(5, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 22, 16, "Captured variable 'c' might be uninitialized at this point");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 24, 21, "Usage of uninitialized variable 'c'");
            testHelper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 27, 8, "Captured variable 'a' might be uninitialized");
            testHelper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 27, 8, "Captured variable 'q' might be uninitialized");
            testHelper.AssertErrorMessage(4, MessageKind.SEMANTIC_ERROR, 30, 13, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void ValidWritelnCall()
        {
            var reporter = new MessageReporter();
            Analyze("valid_writeln_call.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidWritelnCall()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_writeln_call.txt", reporter);
            Assert.AreEqual(4, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 7, 16, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 7, 19, "Usage of uninitialized variable 'b'");
            testHelper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 7, 22, "Usage of uninitialized variable 'c'");
            testHelper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 7, 25, "Usage of uninitialized variable 'd'");
        }

        [TestMethod()]
        public void ValidReadCall()
        {
            var reporter = new MessageReporter();
            Analyze("valid_read_call.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidReadCall()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_read_call.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 6, 13, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void ValidNormalFunctionCalls()
        {
            var reporter = new MessageReporter();
            Analyze("valid_function_calls.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }
        
        [TestMethod()]
        public void InvalidNormalFunctionCalls()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_function_calls.txt", reporter);
            Assert.AreEqual(4, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 14, 22, "Usage of uninitialized variable 'c'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 22, 27, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 22, 30, "Usage of uninitialized variable 'b'");
            testHelper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 22, 34, "Usage of uninitialized variable 'b'");
        }
                   
        [TestMethod()]
        public void InvalidArrayIndexes()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_array_indexes.txt", reporter);
            Assert.AreEqual(5, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 4, 23, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 5, 23, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 9, 10, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(3, MessageKind.SEMANTIC_ERROR, 10, 10, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(4, MessageKind.SEMANTIC_ERROR, 10, 22, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void InvalidReturnStatements()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_returns.txt", reporter);
            Assert.AreEqual(3, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 6, 23, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 21, 23, "Usage of uninitialized variable 'c'");
            testHelper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 16, 8, "Captured variable 'b' might be uninitialized at this point");

        }

        [TestMethod()]
        public void InvalidAsserts()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_asserts.txt", reporter);
            Assert.AreEqual(3, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 3, 15, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 11, 8, "Captured variable 'a' might be uninitialized at this point");
            testHelper.AssertErrorMessage(2, MessageKind.SEMANTIC_ERROR, 14, 15, "Usage of uninitialized variable 'b'");
        }

        [TestMethod()]
        public void InvalidJump()
        {
            var reporter = new MessageReporter();
            Analyze("invalid_jumps.txt", reporter);
            Assert.AreEqual(2, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, MessageKind.SEMANTIC_ERROR, 3, 11, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, MessageKind.SEMANTIC_ERROR, 4, 22, "Usage of uninitialized variable 'a'");
        }


        /*
            Test for a bug that was found during manual testing 
            Function calls were not updating the capture list correctly
        */
        [TestMethod()]
        public void CallingNonEnclosedFunctionThatCapturesVariables()
        {
            var reporter = new MessageReporter();
            Analyze("call_function_that_captures_from_other_function.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        /*
            Test for a bug that was found during manual testing 
            Control flow analysis did not consider function parameters to be initialized
            when capturing them in inner context
        */
        [TestMethod()]
        public void CaptureFunctionParameters()
        {
            var reporter = new MessageReporter();
            Analyze("capture_function_parameter.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }
    }
}