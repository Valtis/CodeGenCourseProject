using System;

namespace CodeGenCourseProject.AST
{
    public class GreaterThanOrEqualNode : ASTNode
    {
        public GreaterThanOrEqualNode(int line, int column, ASTNode lhs, ASTNode rhs) : base(line, column)
        {
            Children.Add(lhs);
            Children.Add(rhs);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("GreaterThanOrEqualNode", "");
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
