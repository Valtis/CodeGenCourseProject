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
        private void Analyze(string name, ErrorReporter reporter)
        {
            var lexer = new Lexer(@"..\..\CFG\Analysis\" + name, reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);
            Assert.AreEqual(0, reporter.Errors.Count);

            var tacGenerator = new TACGenerator();
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
            var reporter = new ErrorReporter();
            Analyze("uninitialized_variable_no_branches.txt", reporter);

            Assert.AreEqual(1, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 4, 13, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void SelfAssignmentIsError()
        {
            var reporter = new ErrorReporter();
            Analyze("self_assignment.txt", reporter);

            Assert.AreEqual(1, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 3, 8, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void UsingVariableBeforeInitializationInSameBasicBlockIsError()
        {
            var reporter = new ErrorReporter();
            Analyze("usage_before_assignment.txt", reporter);

            Assert.AreEqual(1, reporter.Errors.Count);

            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 3, 13, "Usage of uninitialized variable 'a'");
        }
        
        [TestMethod()]
        public void InitalizedVariablesCauseNoErrorsWhenNoBranching()
        {
            var reporter = new ErrorReporter();
            Analyze("initialized_variable_no_branches.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);

        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_if_else_initialization.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);

        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization2()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_if_branch_only_initialization.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization3()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_else_branch_only_initialization.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization4()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_if_branch_initialization_with_dead_else.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidIfStatementVariableInitialization5()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_nested_if_statements.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidIfStatementVariableInitialization()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_if_branch_only_initialization.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 9, 17, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void InvalidIfStatementVariableInitialization2()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_if_branch_only_initialization_with_else_branch.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 12, 13, "Usage of uninitialized variable 'b'");
        }

        [TestMethod()]
        public void InvalidIfStatementVariableInitialization3()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_nested_if_statements.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 24, 13, "Usage of uninitialized variable 'c'");
        }

        [TestMethod()]
        public void InvalidIfStatementVariableInitialization4()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_variable_used_in_if_blocks_before_initialization_in_block.txt", reporter);
            Assert.AreEqual(2, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 6, 21, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, Error.SEMANTIC_ERROR, 11, 21, "Usage of uninitialized variable 'b'");
        }
        
        [TestMethod()]
        public void UsingUninitalizedVariableInIfElseBlocksIsError()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_variable_if_block.txt", reporter);
            Assert.AreEqual(2, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 7, 21, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, Error.SEMANTIC_ERROR, 9, 25, "Usage of uninitialized variable 'a'");
        }
        
        [TestMethod()]
        public void UninitializedVariableIsErrorInIfCondition()
        {
            var reporter = new ErrorReporter();
            Analyze("uninitialized_variable_in_if_condition.txt", reporter);
            Assert.AreEqual(2, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);

            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 4, 11, "Usage of uninitialized variable 'a'");
            testHelper.AssertErrorMessage(1, Error.SEMANTIC_ERROR, 4, 19, "Usage of uninitialized variable 'b'");
        }


        [TestMethod()]
        public void ValidWhileStatementVariableUsage()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_while_statement.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
        }

        [TestMethod()]
        public void ValidWhileStatement2()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_while_statement2.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
        }

        [TestMethod()]
        public void ValidWhileStatement3()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_while_statement3.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
        }

        [TestMethod()]
        public void InvalidWhileStatement1()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_while_statement1.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 4, 14, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void InvalidWhileStatement2()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_while_statement2.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 6, 21, "Usage of uninitialized variable 'b'");
        }

        [TestMethod()]
        public void InvalidWhileStatement3()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_while_statement3.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 7, 13, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void InvalidWhileStatement4()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_while_statement4.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 6, 30, "Usage of uninitialized variable 'a'");
        }

        [TestMethod()]
        public void InvalidWhileStatement5()
        {
            var reporter = new ErrorReporter();
            Analyze("invalid_while_statement5.txt", reporter);
            Assert.AreEqual(1, reporter.Errors.Count);
            var testHelper = new TestHelper(reporter);
            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 28, 13, "Usage of uninitialized variable 'c'");
        }

        [TestMethod()]
        public void ValidArrays()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_arrays.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidProcedure()
        {
            var reporter = new ErrorReporter();
            Analyze("valid_variables_in_procedures.txt", reporter);
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void UninitializedVariableIsErrorInFunctionCalls()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InitializedVariableCausesNoErrorInFunctionCall()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void FunctionWithoutReturnStatementInAllBranchesIsError()
        {
            Assert.Fail();
        }
        [TestMethod()]
        public void FunctionWithReturnStatementInAllBranchesIsOk()
        {
            Assert.Fail();
        }
    }
}