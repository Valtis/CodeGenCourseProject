using CodeGenCourseProject.TAC;
using CodeGenCourseProject.TAC.Values;
using System;
using System.Collections.Generic;

namespace CodeGenCourseProject.CFG
{
    public class CFGGenerator
    {
        private IList<Function> functions;

        public CFGGenerator(IList<Function> functions)
        {
            this.functions = functions;
        }
                
        public IDictionary<string, CFGraph> GenerateCFG()
        {
            var cfgGraphs = new Dictionary<string, CFGraph>();
            foreach (var function in functions)
            {
                var graph = GenerateCFG(function);
                cfgGraphs.Add(function.Name, graph);
            }

            return cfgGraphs;
        }


        private CFGraph GenerateCFG(Function function)
        {
            var blocks = CreateBlocks(function);

            Console.WriteLine("\n****BASIC BLOCKS****\n");
            foreach (var block in blocks)
            {
                Console.WriteLine("Block " + block.ID);
                for (int i = block.Start; i <= block.End; ++i)
                {
                    Console.WriteLine("" + i + ": " + function.Statements[i]);
                }

                Console.WriteLine("");
            }

            var adjacencyList= CreateEdges(function, blocks);
            Console.WriteLine("");
            for (int i = 0; i < adjacencyList.Count; ++i)
            {
                Console.WriteLine("Neighbours for block " + i);
                
                foreach (var j in adjacencyList[i])
                {
                    Console.Write("" + j + " ");
                }


                Console.WriteLine();
            }

            return new CFGraph(blocks, adjacencyList);
        }

        private List<BasicBlock> CreateBlocks(Function function)
        {
            int id = 0;
            var blocks = new List<BasicBlock>();
            var starts = new List<int>();
            var ends = new List<int>();
            int start = 0;
            int pos = 0;

            foreach (var statement in function.Statements)
            {
                if (IsJumpOrReturn(statement)&& pos != 0)
                {
                    var startPos = pos + 1;
                    var endPos = pos;
                    var block = new BasicBlock(start, pos, id++);
                    blocks.Add(block);
                    start = startPos;
                }
                else if (statement.RightOperand is TACLabel)
                {
                    var endPos = pos - 1;
                    if (start <= endPos)
                    {
                        var block = new BasicBlock(start, endPos, id++);
                        blocks.Add(block);
                    }
                    start = pos;
                }
                ++pos;
            }
            if (start < function.Statements.Count)
            {
                blocks.Add(new BasicBlock(start, pos - 1, id++));
            }
            return blocks;
        }
                
        private IList<IList<int>> CreateEdges(Function function, IList<BasicBlock> blocks)
        {
            var adjacencyList = new List<IList<int>>();

            for (int i = 0; i < blocks.Count; ++i)
            {
                adjacencyList.Add(new List<int>());
            }

            int pos = 0;
            foreach (var block in blocks)
            {
                var rightOperand = function.Statements[block.End].RightOperand;
                // undconditional jump, only edge to the jump target
                if (rightOperand is TACJump)
                {
                    var jumpTarget = ((TACJump)rightOperand).Label;
                    var destBlock = GetJumpTarget(function, jumpTarget);
                    adjacencyList[pos].Add(GetDestinationBlockID(blocks, destBlock));
                }
                // conditional jump, may have edges to the jump target and next block
                else if (rightOperand is TACJumpIfFalse)
                {
                    // if condition is true/false, only one edge will be present
                    var jump = (TACJumpIfFalse)rightOperand;
                    var condition = jump.Condition;
                    var destBlock = GetJumpTarget(function, jump.Label);

                    var boolean = condition as TACBoolean;

                    // if we have true value, or non-boolean value, add the next block as that 
                    // can be entered
                    if (boolean == null || boolean.Value == true)
                    {
                        adjacencyList[pos].Add(GetDestinationBlockID(blocks, block.End + 1));
                    }
                    // if we have false value or non-boolean value, the else block can be entered
                    if (boolean == null || boolean.Value == false)
                    {
                        adjacencyList[pos].Add(GetDestinationBlockID(blocks, destBlock));
                    }

                }
                // edge to end block
                else if (rightOperand is TACReturn)
                {
                    adjacencyList[pos].Add(GetDestinationBlockID(blocks, CFGraph.END_BLOCK_ID));
                }
                // fall through
                else
                {

                    adjacencyList[pos].Add(GetDestinationBlockID(blocks, block.End + 1));
                }
                pos++;
            }

            return adjacencyList;
        }

        private int GetJumpTarget(Function function, TACLabel label)
        {
            int pos = 0;
            foreach (var statement in function.Statements)
            {
                if (label.Equals(statement.RightOperand))
                {
                    return pos;
                }
                pos++;
            }

            throw new InternalCompilerError("Invalid TAC label " + label);
        }

        private int GetDestinationBlockID(IList<BasicBlock> blocks, int blockStart)
        {
            foreach (var dest in blocks)
            {
                if (dest.Start == blockStart)
                {
                    return dest.ID;
                }
            }
            return CFGraph.END_BLOCK_ID;
        }

        private bool IsJumpOrReturn(TACStatement statement)
        {
            return statement.RightOperand is TACJump 
                || statement.RightOperand is TACJumpIfFalse
                || statement.RightOperand is TACReturn;
        }
    }
}
