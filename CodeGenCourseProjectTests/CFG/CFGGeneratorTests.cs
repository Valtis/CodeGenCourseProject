using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Parsing;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC;
using System.Collections.Generic;

namespace CodeGenCourseProject.CFG.Tests
{
    [TestClass()]
    public class CFGGeneratorTests
    {
        private IDictionary<string, CFGraph> GetCFGGraph(string name)
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\CFG\" + name, reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);
            Assert.AreEqual(0, reporter.Errors.Count);

            var tacGenerator = new TACGenerator();
            node.Accept(tacGenerator);

            var cfgGenerator = new CFGGenerator(tacGenerator.Functions);
            return cfgGenerator.GenerateCFG();
        }

        [TestMethod()]
        public void BranclessProgramGeneratesCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("no_branching.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(1, cfg.Blocks.Count);
            Assert.AreEqual(1, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 5, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void SimpleIfStatementGeneratesCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("simple_if_statement.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(3, cfg.Blocks.Count);
            Assert.AreEqual(3, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 2, new List<int> { 1, 2 });
            AssertBlock(cfg, 1, 3, 5, new List<int> { 2 });
            AssertBlock(cfg, 2, 6, 6, new List<int> { CFGraph.END_BLOCK_ID });
        }


        [TestMethod()]
        public void IfElseStatementGeneratesCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("if_else_statement.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(4, cfg.Blocks.Count);
            Assert.AreEqual(4, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 2, new List<int> { 1, 2 });
            AssertBlock(cfg, 1, 3, 4, new List<int> { 3 });
            AssertBlock(cfg, 2, 5, 8, new List<int> { 3 });
            AssertBlock(cfg, 3, 9, 10, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void IfStatementWithTrueConditionGeneratesCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("if_statement_with_true_condition.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(4, cfg.Blocks.Count);
            Assert.AreEqual(4, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 2, new List<int> { 1 });
            AssertBlock(cfg, 1, 3, 5, new List<int> { 3 });
            AssertBlock(cfg, 2, 6, 8, new List<int> { 3 });
            AssertBlock(cfg, 3, 9, 11, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void IfStatementWithFalseConditionGeneratesCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("if_statement_with_false_condition.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(4, cfg.Blocks.Count);
            Assert.AreEqual(4, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 2, new List<int> { 2 });
            AssertBlock(cfg, 1, 3, 5, new List<int> { 3 });
            AssertBlock(cfg, 2, 6, 8, new List<int> { 3 });
            AssertBlock(cfg, 3, 9, 11, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void NestedIfStatementsGenerateCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("nested_if_statements.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(10, cfg.Blocks.Count);
            Assert.AreEqual(10, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 3, new List<int> { 1, 5 });
            AssertBlock(cfg, 1, 4, 5, new List<int> { 2, 3 });
            AssertBlock(cfg, 2, 6, 7, new List<int> { 4 });
            AssertBlock(cfg, 3, 8, 9, new List<int> { 4 });
            AssertBlock(cfg, 4, 10, 11, new List<int> { 9 });
            AssertBlock(cfg, 5, 12, 14, new List<int> { 6, 7 });
            AssertBlock(cfg, 6, 15, 18, new List<int> { 8 });
            AssertBlock(cfg, 7, 19, 21, new List<int> { 8 });
            AssertBlock(cfg, 8, 22, 24, new List<int> { 9 });
            AssertBlock(cfg, 9, 25, 27, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void WhileLoopGeneratesCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("while_statement.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(4, cfg.Blocks.Count);
            Assert.AreEqual(4, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 0, new List<int> { 1});
            AssertBlock(cfg, 1, 1, 3, new List<int> { 2,3 });
            AssertBlock(cfg, 2, 4, 7, new List<int> { 1 });
            AssertBlock(cfg, 3, 8, 8, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void WhileLoopWithTrueConditionGeneratesCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("while_statement_with_true_condition.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(4, cfg.Blocks.Count);
            Assert.AreEqual(4, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 0, new List<int> { 1 });
            AssertBlock(cfg, 1, 1, 2, new List<int> { 2 });
            AssertBlock(cfg, 2, 3, 6, new List<int> { 1 });
            AssertBlock(cfg, 3, 7, 8, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void WhileLoopWithFalseConditionGeneratesCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("while_statement_with_false_condition.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(4, cfg.Blocks.Count);
            Assert.AreEqual(4, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 0, new List<int> { 1 });
            AssertBlock(cfg, 1, 1, 2, new List<int> { 3 });
            AssertBlock(cfg, 2, 3, 6, new List<int> { 1 });
            AssertBlock(cfg, 3, 7, 8, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void WhileLoopWithIfStatementsGenerateCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("while_and_if_statements.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(10, cfg.Blocks.Count);
            Assert.AreEqual(10, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 1, new List<int> { 1 });
            AssertBlock(cfg, 1, 2, 4, new List<int> { 2, 9 });
            AssertBlock(cfg, 2, 5, 10, new List<int> { 3, 4 });
            AssertBlock(cfg, 3, 11, 12, new List<int> { 8 });
            AssertBlock(cfg, 4, 13, 13, new List<int> { 5 });
            AssertBlock(cfg, 5, 14, 16, new List<int> { 6, 7 });
            AssertBlock(cfg, 6, 17, 19, new List<int> { 5, });
            AssertBlock(cfg, 7, 20, 20, new List<int> { 8 });
            AssertBlock(cfg, 8, 21, 23, new List<int> { 1 });
            AssertBlock(cfg, 9, 24, 24, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void ProceduresGenerateCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("procedures.txt");
            Assert.AreEqual(3, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(0, cfg.Blocks.Count);
            Assert.AreEqual(0, cfg.AdjacencyList.Count);

            cfg = graphs["__a0__"];
            Assert.AreEqual(1, cfg.Blocks.Count);
            Assert.AreEqual(1, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 2, new List<int> { CFGraph.END_BLOCK_ID });


            cfg = graphs["__b2__"];
            Assert.AreEqual(5, cfg.Blocks.Count);
            Assert.AreEqual(5, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 4, new List<int> { 1, 3 });
            AssertBlock(cfg, 1, 5, 6, new List<int> { CFGraph.END_BLOCK_ID });
            AssertBlock(cfg, 2, 7, 9, new List<int> { 4 });
            AssertBlock(cfg, 3, 10, 12, new List<int> { CFGraph.END_BLOCK_ID });
            AssertBlock(cfg, 4, 13, 14, new List<int> { CFGraph.END_BLOCK_ID });
        }

        [TestMethod()]
        public void FunctionsGenerateCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("functions.txt");
            Assert.AreEqual(2, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(0, cfg.Blocks.Count);
            Assert.AreEqual(0, cfg.AdjacencyList.Count);

            cfg = graphs["__a0__"];
            Assert.AreEqual(3, cfg.Blocks.Count);
            Assert.AreEqual(3, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 3, new List<int> { 1, 2 });
            AssertBlock(cfg, 1, 4, 4, new List<int> { CFGraph.END_BLOCK_ID });
            AssertBlock(cfg, 2, 5, 7, new List<int> { CFGraph.END_BLOCK_ID });
        }
        private static void AssertBlock(CFGraph cfg, int block, int start, int end, IList<int> children)
        {
            Assert.AreEqual(children.Count, cfg.AdjacencyList[block].Count);
            foreach (var child in children)
            {
                Assert.IsTrue(cfg.AdjacencyList[block].Contains(child));
            }
            Assert.AreEqual(start, cfg.Blocks[block].Start);
            Assert.AreEqual(end, cfg.Blocks[block].End);
            Assert.AreEqual(block, cfg.Blocks[block].ID);
        }
    }
}