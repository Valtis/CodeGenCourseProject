using System;
using CodeGenCourseProject.Tokens;

namespace CodeGenCourseProject.AST
{
    public class ArrayIndexNode : ASTNode
    {
        
        public ArrayIndexNode(int line, int column, IdentifierNode array, ASTNode index) : base(line, column)
        {
            Children.Add(array);
            Children.Add(index);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("ArrayIndexNode", ((IdentifierNode)Children[0]).Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayIndexNode && 
                ((IdentifierNode)Children[0]).Value.Equals(
                    ((IdentifierNode)((ArrayIndexNode)obj).Children[0]).Value);
        }

        public override int GetHashCode()
        {
            return ((IdentifierNode)Children[0]).Value.GetHashCode();
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
