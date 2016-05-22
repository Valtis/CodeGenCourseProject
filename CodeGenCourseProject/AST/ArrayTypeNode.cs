using System;

namespace CodeGenCourseProject.AST
{
    public class ArrayTypeNode : ASTNode
    {
        public ArrayTypeNode(int line, int column, IdentifierNode type, ASTNode expression) : base(line, column)
        {
            Children.Add(type);
            if (expression != null)
            {
                Children.Add(expression);
            }
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("ArrayTypeNode", "");
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
