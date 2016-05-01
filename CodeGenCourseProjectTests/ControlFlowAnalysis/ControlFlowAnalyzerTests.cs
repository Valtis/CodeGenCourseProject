using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.ControlFlowAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.Parsing;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC;
using CodeGenCourseProject.CFG;
using CodeGenCourseProjectTests;

namespace CodeGenCourseProject.ControlFlowAnalysis.Tests
{
    [TestClass()]
    public class ControlFlowAnalyzerTests
    {
        private void Analyze(string name, ErrorReporter reporter)
        {
            var lexer = new Lexer(@"..\..\ControlFlowAnalysis\" + name, reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);
            Assert.AreEqual(0, reporter.Errors.Count);

            var tacGenerator = new TACGenerator();
            node.Accept(tacGenerator);

            var cfgGenerator = new CFGGenerator();

            var analyzer = new ControlFlowAnalyzer(
                reporter,
                tacGenerator.Functions,
                cfgGenerator.GenerateCFG(tacGenerator.Functions));
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
        public void ValidFiStatementVariableInitialization4()
        {
            // assignment in if branch, when condition is true, and else branch is present
            Assert.Fail();
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
            testHelper.AssertErrorMessage(0, Error.SEMANTIC_ERROR, 9, 17, "Usage of uninitialized variable 'a'");
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
        public void UninitializedVariableIsErrorInIfCondition()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void InitializedVariableCausesNoErrorInIfCondition()
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