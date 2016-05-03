using CodeGenCourseProject.CFG;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.TAC;
using CodeGenCourseProject.TAC.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenCourseProject.CFG.Analysis
{
    public class CFGAnalyzer
    {
        private readonly ErrorReporter reporter;
        private readonly IList<Function> functions;
        private readonly IDictionary<string, CFGraph> graphs;
        private readonly ISet<int> unreachableBlocks;
        public CFGAnalyzer(ErrorReporter reporter, IList<Function> functions, IDictionary<string, CFGraph> graphs)
        {
            this.reporter = reporter;
            this.functions = functions;
            this.graphs = graphs;
            this.unreachableBlocks = new HashSet<int>();
        }

        public void Analyze()
        {
            foreach (var function in functions)
            {
                AnalyzeFunction(function);
            }
        }

        private void AnalyzeFunction(Function function)
        {
            var graph = graphs[function.Name];
            if (graph.Blocks.Count == 0)
            {
                return;
            }
            DetectDeadBlocks(function, graph);
            AnalyzeVariableUsage(function, graph);
        }

        /*
         * Remove any blocks that are not connected to the first block to prevent them
         * from affecting e.g. variable initialization analysis
         */
        private void DetectDeadBlocks(Function function, CFGraph graph)
        {
            var handledBlocks = new HashSet<int>();
            var blockQueue = new Queue<int>();
            blockQueue.Enqueue(0);
            while (blockQueue.Count != 0)
            {
                var block = blockQueue.Dequeue();
                handledBlocks.Add(block);

                foreach (var child in graph.AdjacencyList[block])
                {
                    if (!(handledBlocks.Contains(child)) && child != CFGraph.END_BLOCK_ID)
                    {
                        blockQueue.Enqueue(child);
                    }
                }
            }
            
            unreachableBlocks.UnionWith(Enumerable.Range(0, graph.Blocks.Count));
            unreachableBlocks.ExceptWith(handledBlocks);

            foreach (var unreachable in unreachableBlocks)
            {
                var statement = function.Statements[graph.Blocks[unreachable].Start];
                int pos = graph.Blocks[unreachable].Start + 1;

                // labels in general carry no meaningful information.
                while (statement.RightOperand is TACLabel)
                {
                    if (pos > graph.Blocks[unreachable].End)
                    {
                        return;
                    }
                    statement = function.Statements[pos++];
                }
                reporter.ReportError(
                    Error.WARNING,
                    "Unreachable code",
                    statement.Line,
                    statement.Column);
            }            
        }

        private void AnalyzeVariableUsage(Function function, CFGraph graph)
        {
            HandleVariableInitializations(function, graph);
            

            foreach (var block in graph.Blocks)
            {
                // don't care about unreachable blocks
                if (unreachableBlocks.Contains(block.ID))
                {
                    continue;
                }

                for (int i = block.Start; i <= block.End; ++i)
                {
                    CheckInitialization(function.Statements[i], block);
                }
            }
        }

        private void HandleVariableInitializations(Function function, CFGraph graph)
        {
            foreach (var block in graph.Blocks)
            {
                FindBlockVariableInitializations(function, block);
            }

            while (DefiniteVariableAssignment(graph, 0, new HashSet<int>()));
        }

        private void FindBlockVariableInitializations(Function function, BasicBlock block)
        {
            for (int i = block.Start; i <= block.End; ++i)
            {
                var dest = function.Statements[i].Destination;
                if (dest != null && dest is TACIdentifier)
                {
                    block.VariableInitializations.Add((TACIdentifier)dest);
                }
            }

            // entry block - add function arguments as visible
            if (block.ID == 0)
            {
                foreach (var param in function.Parameters)
                {

                    block.VariableInitializations.Add(param.Identifier);
                }
            }
        }

        private bool DefiniteVariableAssignment(CFGraph graph, int id, ISet<int> handledBlocks)
        {
            if (handledBlocks.Contains(id))
            {
                return false;
            }
            
            handledBlocks.Add(id);

            var changes = false;
            if (id != CFGraph.END_BLOCK_ID)
            {
                foreach (var child in graph.AdjacencyList[id])
                {
                    // ignore parents that are after this block, as this indicates a cycle; this parent
                    // may actually depend on this block for definite initializations -> handle later
                    changes = changes || DefiniteVariableAssignment(graph, child, handledBlocks);
                }
            }

            var parents = GetParentBlocks(graph, id);
            ISet<TACIdentifier> definiteInitializations = null; 
            foreach (var parent in parents)
            {
                // ignore parents that are after this block, as this indicates a cycle
                if (id != CFGraph.END_BLOCK_ID && (graph.Blocks[parent].Start > graph.Blocks[id].End))
                {
                    continue;
                }

                // control flow from the unreachable parents will not affect variable
                // initialization
                if (unreachableBlocks.Contains(parent))
                {
                    continue;
                }

                var definiteParentInitializations = new HashSet<TACIdentifier>();

                definiteParentInitializations.UnionWith(graph.Blocks[parent].ParentInitializations);
                definiteParentInitializations.UnionWith(graph.Blocks[parent].VariableInitializations);
                if (definiteInitializations == null)
                {
                    definiteInitializations = definiteParentInitializations;
                }
                else
                {
                    definiteInitializations.IntersectWith(definiteParentInitializations);
                }
            }

            if (id != CFGraph.END_BLOCK_ID && definiteInitializations != null)
            {
                var block = graph.Blocks[id];
                var length = block.ParentInitializations.Count;
                block.ParentInitializations.UnionWith(definiteInitializations);
                var newLength = block.ParentInitializations.Count;
                changes = changes || newLength != length;
            }
            
            return changes;
        }

        private void CheckInitialization(TACStatement statement, BasicBlock block)
        {
            CheckInitialization(statement.LeftOperand, block);
            CheckInitialization(statement.RightOperand, block);
        }

        private void CheckInitialization(TACValue value, BasicBlock block)
        {
            if (value is TACIdentifier)
            {
                var identifier = (TACIdentifier)value;
                if (block.ParentInitializations.Contains(identifier))
                {
                    return;
                }
                else if (block.VariableInitializations.Contains(identifier))
                {
                    foreach (var init in block.VariableInitializations)
                    {
                        if (init.Equals(identifier))
                        {
                            if (init.Line < identifier.Line || identifier.UnmangledName == "__t")
                            {
                                return;
                            }
                            break;
                        }
                    }
                }

                // arrays are zero-initialized and thus can be used without previous
                // assignments
                if (identifier.Type.Contains("Array<"))
                {
                    return;
                }

                ReportUninitializedVariable(identifier);
            }
        }

        private void ReportUninitializedVariable(TACIdentifier identifier)
        {
            reporter.ReportError(
                Error.SEMANTIC_ERROR,
                "Usage of uninitialized variable '" + identifier.UnmangledName + "'", 
                identifier.Line,
                identifier.Column);

            reporter.ReportError(
                Error.NOTE_GENERIC,
                "There exists one or more control flow paths where this variable isn't initialized",
                identifier.Line,
                identifier.Column);
        }



        private IList<int> GetParentBlocks(CFGraph graph, int id)
        {
            var parents = new List<int>();
            for (int i = 0; i < graph.Blocks.Count; ++i)
            {
                foreach (var neighbor in graph.AdjacencyList[i])
                {
                    if (neighbor == id)
                    {
                        parents.Add(i);
                        break;
                    }
                }
            }

            return parents;
        }
    }
}
