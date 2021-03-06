﻿using System;

namespace CodeGenCourseProject.AST
{
    public class BlockNode : ASTNode
    {
        private SymbolTableLevel symbols;
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
            return new Tuple<string, string>("BlockNode", "");
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }

        internal void SetSymbols(SymbolTableLevel symbols)
        {
            this.symbols = symbols;
        }

        internal SymbolTableLevel GetSymbols()
        {
            return symbols;
        }

    }
}
