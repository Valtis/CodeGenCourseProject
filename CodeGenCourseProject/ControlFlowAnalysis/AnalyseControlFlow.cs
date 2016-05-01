using CodeGenCourseProject.CFG;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.TAC;
using CodeGenCourseProject.TAC.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenCourseProject.ControlFlowAnalysis
{
    public class ControlFlowAnalyzer
    {
        private readonly ErrorReporter reporter;
        private readonly IList<Function> functions;
        private readonly IDictionary<string, CFGGraph> graphs;
        public ControlFlowAnalyzer(ErrorReporter reporter, IList<Function> functions, IDictionary<string, CFGGraph> graphs)
        {
            this.reporter = reporter;
            this.functions = functions;
            this.graphs = graphs;
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
            RemoveDeadBlocksFromGraph(function, graphs[function.Name]);
            AnalyzeVariableUsage(function, graphs[function.Name]);
        }

        /*
         * Remove any blocks that are not connected to the first block to prevent them
         * from affecting e.g. variable initialization analysis
         */
        private void RemoveDeadBlocksFromGraph(Function function, CFGGraph graph)
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
                    if (!(handledBlocks.Contains(child)) && child != CFGGraph.END_BLOCK_ID)
                    {
                        blockQueue.Enqueue(child);
                    }
                }
            }

            var unreachableBlocks = new HashSet<int>();
            unreachableBlocks.UnionWith(Enumerable.Range(0, graph.Blocks.Count));
            unreachableBlocks.ExceptWith(handledBlocks);

            foreach (var unreachable in unreachableBlocks)
            {
                var firstStatement = function.Statements[graph.Blocks[unreachable].Start];
                reporter.ReportError(
                    Error.WARNING,
                    "Unreachable code",
                    firstStatement.Line,
                    firstStatement.Column);

                graph.AdjacencyList[unreachable].Clear();
            }            
        }

        private void AnalyzeVariableUsage(Function function, CFGGraph graph)
        {
            HandleVariableInitializations(function, graph);

            foreach (var block in graph.Blocks)
            {
          
                for (int i = block.Start; i <= block.End; ++i)
                {
                    CheckInitialization(function.Statements[i], block);
                }

            }

        }

        private void HandleVariableInitializations(Function function, CFGGraph graph)
        {
            foreach (var block in graph.Blocks)
            {
                FindBlockVariableInitializations(function, block);
            }

            DefiniteVariableAssignment(graph, CFGGraph.END_BLOCK_ID, new HashSet<int>());
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
            
        }

        private void DefiniteVariableAssignment(CFGGraph graph, int id, ISet<int> handledBlocks)
        {
            if (handledBlocks.Contains(id))
            {
                return;
            }
            
            handledBlocks.Add(id);
            var parents = GetParentBlocks(graph, id);

            if (parents.Count == 0)
            {
                return;
            }

            foreach (var parent in parents)
            {
                DefiniteVariableAssignment(graph, parent, handledBlocks);
            }

            ISet<TACIdentifier> definiteInitializations = null; 
            foreach (var parent in parents)
            {

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

            if (id != CFGGraph.END_BLOCK_ID)
            {
                graph.Blocks[id].ParentInitializations = definiteInitializations;
            }
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
        }



        private IList<int> GetParentBlocks(CFGGraph graph, int id)
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
