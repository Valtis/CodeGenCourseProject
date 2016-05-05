using CodeGenCourseProject.CFG;
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
            unreachableBlocks.Clear();
            var graph = graphs[function.Name];
            if (graph.Blocks.Count == 0)
            {
                return;
            }
            DetectDeadBlocks(function, graph);
            AnalyzeVariableUsage(function, graph);
            AnalyzeCapturedVariableInitialization(function, graph);
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
                    CheckInitialization(function, function.Statements[i], block);
                }
                

                // check that reference parameters are assigned into by the time we
                // are exiting the function\procedure
                if (function.Name != TACGenerator.ENTRY_POINT && 
                    graph.AdjacencyList[block.ID].Contains(CFGraph.END_BLOCK_ID))
                {
                    // Below line violates language semantics (reference parameters must
                    // be both readable and writable), so let's just remove it for now.
                    // Code is left for posterity however.             

                    //CheckReferenceAssignments(function, reportedRefParams, block);

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
                else if (function.Statements[i].RightOperand is TACCallRead)
                {
                    var readCall = (TACCallRead)function.Statements[i].RightOperand;
                    foreach (var arg in readCall.Arguments)
                    {
                        if (arg is TACIdentifier)
                        {
                            block.VariableInitializations.Add((TACIdentifier)arg);
                        }
                    }
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

        private void CheckInitialization(Function function, TACStatement statement, BasicBlock block)
        {
            if (statement.Destination is TACArrayIndex)
            {
                CheckInitialization(function, statement.Destination, block);
            }
            CheckInitialization(function, statement.LeftOperand, block);
            CheckInitialization(function, statement.RightOperand, block);
        }

        private void CheckInitialization(Function function, TACValue value, BasicBlock block)
        {
            HandleNonIdentifiers(function, value, block);

            if (value is TACIdentifier)
            {
                var identifier = (TACIdentifier)value;
                var isAssigned = VariableIsAssignedInto(identifier, block);
                if (isAssigned)
                {
                    return;
                }

                // check if value is function parameter or captured variable
                //, as these are always considered to be valid
                if (!isAssigned) { 
                    foreach (var param in function.Parameters)
                    {
                        if (param.Identifier.Equals(identifier))
                        {
                            return;
                        }
                    }

                    foreach (var param in function.CapturedVariables)
                    {
                        if (param.Identifier.Equals(identifier))
                        {
                            return;
                        }
                    }
                }

                // arrays are zero-initialized and thus can be used without previous
                // assignments

                ReportUninitializedVariable(identifier);
            }
        }

        private void HandleNonIdentifiers(Function function, TACValue value, BasicBlock block)
        {
            if (value is TACCallRead)
            {
                // read call initializes variables - skip
                return;
            }

            if (value is TACArrayIndex)
            {
                var index = (TACArrayIndex)value;
                CheckInitialization(function, index.Index, block);
                return;
            }

            if (value is TACArrayDeclaration)
            {
                var decl = (TACArrayDeclaration)value;
                CheckInitialization(function, decl.Expression, block);
                return;
            }

            if (value is TACReturn)
            {
                var decl = (TACReturn)value;
                CheckInitialization(function, decl.Expression, block);
                return;
            }

            if (value is TACAssert)
            {
                var assert = (TACAssert)value;
                CheckInitialization(function, assert.Expression, block);
                return;
            }

            if (value is TACJumpIfFalse)
            {
                var jump = (TACJumpIfFalse)value;
                CheckInitialization(function, jump.Condition, block);
                return;
            }



            if (value is TACCall)
            {
                var call = (TACCall)value;
                // check that the function arguments are initialized
                foreach (var arg in call.Arguments)
                {
                    CheckInitialization(function, arg, block);
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
                            // we need to use the line number of the call, not the 
                            // point where it is used
                            var id = new TACIdentifier(
                                call.Line,
                                call.Column,
                                captured.Identifier.UnmangledName,
                                captured.Identifier.Type,
                                captured.Identifier.Id);

                            if (!VariableIsAssignedInto(id, block))
                            {
                                reporter.ReportError(
                                    Error.SEMANTIC_ERROR,
                                    "Captured variable '" + captured.Identifier.UnmangledName + "' might be uninitialized at this point",
                                    call.Line,
                                    call.Column);

                                reporter.ReportError(
                                    Error.NOTE,
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


        private void CheckReferenceAssignments(Function function, HashSet<string> reportedRefParams, BasicBlock block)
        {
            foreach (var param in function.Parameters)
            {
                if (!param.IsReference)
                {
                    continue;
                }

                // create new TACIdentifier with fake line number, as the assignment 
                // takes line account (assignment must happen before usage; the usage
                // is decided by the line number, function arguments are always above
                // usage -> would always report false
                var ident = new TACIdentifier(
                    int.MaxValue,
                    0,
                    param.Identifier.UnmangledName,
                    param.Identifier.Type,
                    param.Identifier.Id);

                var isAssigned = VariableIsAssignedInto(ident, block);
                if (!isAssigned && !reportedRefParams.Contains(param.Identifier.Name))
                {
                    reportedRefParams.Add(param.Identifier.Name);
                    reporter.ReportError(
                        Error.SEMANTIC_ERROR,
                        "Parameter '" + param.Identifier.UnmangledName + "' may remain uninitialized at function exit",
                        param.Identifier.Line,
                        param.Identifier.Column);

                    reporter.ReportError(
                        Error.NOTE_GENERIC,
                        "Reference parameters are considered out-parameters, and a value must be assigned into one",
                        param.Identifier.Line,
                        param.Identifier.Column);
                }
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
                        Error.SEMANTIC_ERROR,
                        "Not all control pathts return a value in funtion '" +
                        function.UnmangledName + "'",
                        function.Line,
                        function.Column
                        );
                }
            }

            return missingReturnReported;
        }


        private void AnalyzeCapturedVariableInitialization(Function function, CFGraph graph)
        {
         
        }

        private bool VariableIsAssignedInto(TACIdentifier identifier, BasicBlock block)
        {
            if (identifier.Type.Contains("Array<"))
            {
                return true;
            }

            if (block.ParentInitializations.Contains(identifier))
            {
                return true;
            }
            else if (block.VariableInitializations.Contains(identifier))
            {
                foreach (var init in block.VariableInitializations)
                {
                    if (init.Equals(identifier))
                    {
                        // check that assignment actually happens before usage
                        if (init.Line < identifier.Line || identifier.UnmangledName == "__t")
                        {
                            return true;
                        }
                        break;
                    }
                }
            }

            return false;
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
