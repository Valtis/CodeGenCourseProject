using System;

namespace CodeGenCourseProject.AST
{
    public class WhileNode : ASTNode
    {
        public WhileNode(int line, int column, ASTNode expression, ASTNode body) : base(line, column)
        {
            Children.Add(expression);
            Children.Add(body);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("WhileNode", "");
        }
    }
}
