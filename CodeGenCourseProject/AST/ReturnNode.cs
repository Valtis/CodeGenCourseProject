using System;

namespace CodeGenCourseProject.AST
{
    public class ReturnNode : ASTNode
    {
        public ReturnNode(int line, int column) : base(line, column)
        {
        }

        public ReturnNode(int line, int column, ASTNode expression) : base(line, column)
        {
            Children.Add(expression);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("ReturnNode", "");
        }
    }
}
