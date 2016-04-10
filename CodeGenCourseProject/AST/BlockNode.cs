﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class BlockNode : ASTNode
    {
        public BlockNode(int line, int column) : base(line, column)
        {

        }

        public BlockNode(int line, int column, params ASTNode[] nodes) : base(line, column)
        {
            foreach (var node in nodes)
            {
                Children.Add(node);
            }
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            throw new NotImplementedException();
        }
    }
}
