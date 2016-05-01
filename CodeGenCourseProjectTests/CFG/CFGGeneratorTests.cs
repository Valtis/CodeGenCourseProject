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
        private IDictionary<string, CFGGraph> GetCFGGraph(string name)
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

            var cfgGenerator = new CFGGenerator();
            return cfgGenerator.GenerateCFG(tacGenerator.Functions);
        }

        [TestMethod()]
        public void BranclessProgramGeneratesCorrectControlFlowGraph()
        {
            var graphs = GetCFGGraph("no_branching.txt");
            Assert.AreEqual(1, graphs.Count);
            var cfg = graphs["<ENTRY POINT>"];
            Assert.AreEqual(1, cfg.Blocks.Count);
            Assert.AreEqual(1, cfg.AdjacencyList.Count);

            AssertBlock(cfg, 0, 0, 5, new List<int>{ CFGGraph.END_BLOCK_ID });
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
            AssertBlock(cfg, 2, 6, 6, new List<int> { CFGGraph.END_BLOCK_ID });
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
            AssertBlock(cfg, 3, 9, 10, new List<int> { CFGGraph.END_BLOCK_ID });
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
            AssertBlock(cfg, 3, 9, 11, new List<int> { CFGGraph.END_BLOCK_ID });
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
            AssertBlock(cfg, 3, 9, 11, new List<int> { CFGGraph.END_BLOCK_ID });
        }

        private static void AssertBlock(CFGGraph cfg, int block, int start, int end, IList<int> children)
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