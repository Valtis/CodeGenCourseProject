using System;

namespace CodeGenCourseProject.AST
{
    public class AddNode : ASTNode
    {
        public AddNode(int line, int column, ASTNode lhs, ASTNode rhs) : base(line, column)
        {
            Children.Add(lhs);
            Children.Add(rhs);
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("AddNode", "");
        }
    }
}
