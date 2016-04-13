using System;

namespace CodeGenCourseProject.AST
{
    public class GreaterThanNode : ASTNode
    {
        public GreaterThanNode(int line, int column, ASTNode lhs, ASTNode rhs) : base(line, column)
        {
            Children.Add(lhs);
            Children.Add(rhs);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("GreaterThanNode", "");
        }
    }
}
