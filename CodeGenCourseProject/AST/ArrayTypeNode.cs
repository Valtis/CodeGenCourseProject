using System;

namespace CodeGenCourseProject.AST
{
    public class ArrayTypeNode : ASTNode
    {
        public ArrayTypeNode(int line, int column, ASTNode type, ASTNode expression) : base(line, column)
        {
            Children.Add(type);
            Children.Add(expression);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            throw new NotImplementedException();
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
