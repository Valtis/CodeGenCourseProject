﻿using CodeGenCourseProject.CFG;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC;
using CodeGenCourseProject.TAC.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeGenCourseProject.CFG.Analysis
{

    public class CFGAnalyzer
    {
        private readonly MessageReporter reporter;
        private readonly IList<Function> functions;
        private readonly IDictionary<string, CFGraph> graphs;
        private readonly ISet<int> unreachableBlocks;
        public CFGAnalyzer(MessageReporter reporter, IList<Function> functions, IDictionary<string, CFGraph> graphs)
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
            unreachableBlocks.Clear();
            var graph = graphs[function.Name];
            if (graph.Blocks.Count == 0)
            {
                return;
            }
            DetectDeadBlocks(function, graph);
            AnalyzeVariableUsage(function, graph);
        }

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

                // labels and jumps are synthetic nodes, generated by other constructs (if\while).
                // Skip them. Sometimes they form non-reachable blocks, which aren't report-worthy
                while (statement.RightOperand is TACLabel || statement.RightOperand is TACJump)
                {
                    if (pos > graph.Blocks[unreachable].End)
                    {
                        return;
                    }
                    statement = function.Statements[pos++];
                }
                reporter.ReportError(
                    MessageKind.WARNING,
                    "Unreachable code",
                    statement.Line,
                    statement.Column);
            }
        }

        private void AnalyzeVariableUsage(Function function, CFGraph graph)
        {
            HandleVariableInitializations(function, graph);
            var reportedRefParams = new HashSet<string>();
            var missingReturnReported = false;
            foreach (var block in graph.Blocks)
            {
                // don't care about unreachable blocks
                if (unreachableBlocks.Contains(block.ID))
                {
                    continue;
                }

                for (int i = block.Start; i <= block.End; ++i)
                {
                    CheckInitialization(function, function.Statements[i], i, block);
                }


                // check that reference parameters are assigned into by the time we
                // are exiting the function\procedure
                if (function.Name != Generator.ENTRY_POINT &&
                    graph.AdjacencyList[block.ID].Contains(CFGraph.END_BLOCK_ID))
                {

                    missingReturnReported = CheckFunctionReturnStatements(function, missingReturnReported, block);
                }
            }
        }

        private void HandleVariableInitializations(Function function, CFGraph graph)
        {
            foreach (var block in graph.Blocks)
            {
                FindBlockVariableInitializations(function, block);
            }
                        
            // initialization data gets propagated only to node children per pass
            // so we need to repeat this as long as there have been changes
            while (PropagateVariableAssignment(graph, 0, new HashSet<int>())) ;
        }

        /*
         Checks for variable initializations in blocks
        */
        private void FindBlockVariableInitializations(Function function, BasicBlock block)
        {
            for (int i = block.Start; i <= block.End; ++i)
            {
                var dest = function.Statements[i].Destination;
                if (dest != null && dest is TACIdentifier)
                {
                    block.VariableInitializations.Add(new BasicBlock.VariableInitPoint((TACIdentifier)dest, i));
                }
                // arguments for read call are always initialized, so treat them as such
                else if (function.Statements[i].RightOperand is TACCallRead)
                {
                    var readCall = (TACCallRead)function.Statements[i].RightOperand;
                    foreach (var arg in readCall.Arguments)
                    {
                        if (arg is TACIdentifier)
                        {
                            block.VariableInitializations.Add(new BasicBlock.VariableInitPoint((TACIdentifier)arg, i));
                        }
                    }
                }
            }
        }

        /*
         Propagate variable initializations to children, when all parents have the variable 
         initialized (either directly in block, or by all of their parents)

         This requires multiple calls to propagate all data correctly throughout the graph

         Parent blocks that are after the current block are ignored (mostly caused by while-loops).

         */
        private bool PropagateVariableAssignment(CFGraph graph, int id, ISet<int> handledBlocks)
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
                    changes = changes || PropagateVariableAssignment(graph, child, handledBlocks);
                }
            }

            var parents = GetParentBlocks(graph, id);
            ISet<BasicBlock.VariableInitPoint> definiteInitializations = null;
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

                var definiteParentInitializations = new HashSet<BasicBlock.VariableInitPoint>();

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

        // check that variables are initialized before use in a basic block
        private void CheckInitialization(Function function, Statement statement, int currentPoint, BasicBlock block)
        {
            // ensure array index is initialized before use
            if (statement.Destination is TACArrayIndex)
            {
                CheckInitialization(function, statement.Destination, block, null, currentPoint);
            }
            CheckInitialization(function, statement.LeftOperand, block, statement.Destination as TACIdentifier, currentPoint);
            CheckInitialization(function, statement.RightOperand, block, statement.Destination as TACIdentifier, currentPoint);
        }

        private void CheckInitialization(
            Function function, 
            TACValue value, 
            BasicBlock block, 
            TACIdentifier destination, 
            int currentPoint)
        {
            HandleNonIdentifiers(function, value, block, destination, currentPoint);

            if (value is TACIdentifier)
            {
                var identifier = (TACIdentifier)value;
                var isAssigned = CheckAssignment(function, identifier, block, destination, currentPoint);
                if (isAssigned)
                {
                    return;
                }

                ReportUninitializedVariable(identifier);
            }
        }

        // ensures tac values in tac values are checked for correct usage
        private void HandleNonIdentifiers(
            Function function,
            TACValue value, 
            BasicBlock block, 
            TACIdentifier destination, 
            int currentPoint)
        {
            if (value is TACCallRead)
            {
                // read call initializes variables - skip
                return;
            }

            if (value is TACArrayIndex)
            {
                var index = (TACArrayIndex)value;
                CheckInitialization(function, index.Index, block, destination, currentPoint);
                return;
            }

            if (value is TACArrayDeclaration)
            {
                var decl = (TACArrayDeclaration)value;
                CheckInitialization(function, decl.Expression, block, destination, currentPoint);
                return;
            }

            if (value is TACReturn)
            {
                var decl = (TACReturn)value;
                CheckInitialization(function, decl.Expression, block, destination, currentPoint);
                return;
            }

            if (value is TACAssert)
            {
                var assert = (TACAssert)value;
                CheckInitialization(function, assert.Expression, block, destination, currentPoint);
                return;
            }

            if (value is TACJumpIfFalse)
            {
                var jump = (TACJumpIfFalse)value;
                CheckInitialization(function, jump.Condition, block, destination, currentPoint);
                return;
            }
            
            if (value is TACCall)
            {
                var call = (TACCall)value;
                // check that the function arguments are initialized
                foreach (var arg in call.Arguments)
                {
                    CheckInitialization(function, arg, block, destination, currentPoint);
                }

                // check that captured parameters are initialized at this point
                foreach (var f in functions)
                {
                    if (f.Name == call.Function)
                    {
                        foreach (var captured in f.CapturedVariables)
                        {
                            // Assume that any variables this function has captured are 
                            // initialized (will be checked in different call site)
                            if (function.CapturedVariables.Contains(captured))
                            {
                                continue;
                            }

                            // function parameters are always initialized
                            if (function.Parameters.Contains(captured))
                            {
                                continue;
                            }

                            // we need to use the line number of the call, not the 
                            // point where it is used
                            var id = new TACIdentifier(
                                call.Line,
                                call.Column,
                                captured.Identifier.UnmangledName,
                                captured.Identifier.Type,
                                captured.Identifier.Id);

                            if (!CheckLocalParameterAssignment(id, block, destination, currentPoint))
                            {
                                reporter.ReportError(
                                    MessageKind.SEMANTIC_ERROR,
                                    "Captured variable '" + captured.Identifier.UnmangledName + "' might be uninitialized at this point",
                                    call.Line,
                                    call.Column);

                                reporter.ReportError(
                                    MessageKind.NOTE,
                                    "Variable is used here",
                                    captured.Identifier.Line,
                                    captured.Identifier.Column);
                            }
                        }
                    }
                }
                return;
            }
        }
        
        private bool CheckFunctionReturnStatements(Function function, bool missingReturnReported, BasicBlock block)
        {
            if (function.ReturnType != SemanticChecker.VOID_TYPE)
            {
                if (!(function.Statements[block.End].RightOperand is TACReturn) &&
                    !missingReturnReported)
                {
                    missingReturnReported = true;
                    reporter.ReportError(
                        MessageKind.SEMANTIC_ERROR,
                        "Not all control paths return a value in function '" +
                        function.UnmangledName + "'",
                        function.Line,
                        function.Column
                        );
                }
            }

            return missingReturnReported;
        }
        
        private bool IsDynamicallyAllocated(TACIdentifier identifier)
        {
            return identifier.Type == SemanticChecker.STRING_TYPE || identifier.Type.Contains(SemanticChecker.ARRAY_PREFIX);
        }

        private bool CheckAssignment(
            Function function, 
            TACIdentifier identifier, 
            BasicBlock block, 
            TACIdentifier destination, 
            int currentPoint,
            bool arraysAreAlwaysValid = true)
        {
            // check if value is function parameter or captured variable
            //, as these are always considered to be valid
            var isAssigned = CheckLocalParameterAssignment(identifier, block, destination, currentPoint, arraysAreAlwaysValid);

            
            if (isAssigned)
            {
                return true;
            }
                
            foreach (var param in function.Parameters)
            {
                if (param.Identifier.Equals(identifier))
                {
                    return true;
                }
            }

            foreach (var param in function.CapturedVariables)
            {
                if (param.Identifier.Equals(identifier))
                {
                    return true;
                }
            }
            
            return false;
        }

        private bool CheckLocalParameterAssignment(
            TACIdentifier identifier, 
            BasicBlock block, 
            TACIdentifier destination, 
            int currentPoint, 
            bool checkArray = true)
        {

            if (checkArray && identifier.Type.Contains(SemanticChecker.ARRAY_PREFIX))
            {
                return true;
            }

            if (block.ParentInitializations.Any(i => i.identifier.Equals(identifier)))
            {
                return true;
            }
            else if (block.VariableInitializations.Any(i => i.identifier.Equals(identifier)))
            {
                foreach (var init in block.VariableInitializations)
                {
                    if (init.identifier.Equals(identifier))
                    {
                        // if init location is the assignment location in this particular statement, 
                        // and we are using the same variable, the variable isn't actually initialized yet 
                        if (destination != null &&
                            init.initPoint == currentPoint &&
                            destination.Equals(identifier))
                        {
                            break;
                        }

                        // check that assignment actually happens before usage
                        if (identifier.UnmangledName != "__t" && init.initPoint >= currentPoint)
                        {
                            break;
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private void ReportUninitializedVariable(TACIdentifier identifier)
        {
            reporter.ReportError(
                MessageKind.SEMANTIC_ERROR,
                "Usage of uninitialized variable '" + identifier.UnmangledName + "'",
                identifier.Line,
                identifier.Column);

            reporter.ReportError(
                MessageKind.NOTE_GENERIC,
                "At least one control flow path exists where this variable remains uninitialized",
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
