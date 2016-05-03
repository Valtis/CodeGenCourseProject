using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.CFG
{
    public class CFGraph
    {
        public const int END_BLOCK_ID = int.MaxValue;
        private readonly IList<BasicBlock> blocks;
        private readonly IList<IList<int>> adjacencyList;

        public IList<IList<int>> AdjacencyList
        {
            get
            {
                return adjacencyList;
            }
        }

        public IList<BasicBlock> Blocks
        {
            get
            {
                return blocks;
            }
        }

        public CFGraph(IList<BasicBlock> blocks, IList<IList<int>> adjacencyList)
        {
            this.blocks = blocks;
            this.adjacencyList = adjacencyList;
        }
    }
}
