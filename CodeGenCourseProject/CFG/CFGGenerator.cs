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
            var adjacencyList = CreateEdges(function, blocks);

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
                if (IsJumpOrReturn(statement))
                {
                    var startPos = pos + 1;
                    var endPos = pos;
                    var block = new BasicBlock(start, pos, id++);
                    blocks.Add(block);
                    start = startPos;
                }
                else if (statement.Operator == Operator.LABEL)
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
                var op = function.Statements[block.End].Operator;
                var leftOperand = function.Statements[block.End].LeftOperand;
                var rightOperand = function.Statements[block.End].RightOperand;
                // undconditional jump, only edge to the jump target
                if (op == Operator.JUMP)
                {
                    var jumpTarget = ((TACInteger)rightOperand).Value;
                    var destBlock = GetJumpTarget(function, jumpTarget);
                    adjacencyList[pos].Add(GetDestinationBlockID(blocks, destBlock));
                }
                // conditional jump, may have edges to the jump target and next block
                else if (op == Operator.JUMP_IF_FALSE)
                {
                    // if condition is true/false, only one edge will be present
                    var jumpTarget = ((TACInteger)rightOperand).Value;
                    var condition = leftOperand;
                    var destBlock = GetJumpTarget(function, jumpTarget);

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

        private int GetJumpTarget(Function function, int labelId)
        {
            int pos = 0;
            foreach (var statement in function.Statements)
            {
                if (statement.Operator == Operator.LABEL && new TACInteger(labelId).Equals(statement.RightOperand))
                {
                    return pos;
                }
                pos++;
            }

            throw new InternalCompilerError("Invalid TAC label " + labelId);
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

        private bool IsJumpOrReturn(Statement statement)
        {
            return statement.Operator == Operator.JUMP
                || statement.Operator == Operator.JUMP_IF_FALSE
                || statement.RightOperand is TACReturn;
        }



        private static void PrintBlocks(Function function, List<BasicBlock> blocks)
        {
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
        }

        private static void PrintEdges(IList<IList<int>> adjacencyList)
        {
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
        }

    }
}
