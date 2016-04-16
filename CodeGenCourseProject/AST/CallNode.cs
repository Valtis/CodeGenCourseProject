using System;

namespace CodeGenCourseProject.AST
{
    public class CallNode : ASTNode
    { 
        public CallNode(int line, int column, ASTNode name, params ASTNode[] nodes) : base(line, column)
        {
            Children.Add(name);
            foreach (var node in nodes)
            {
                Children.Add(node);
            }
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("CallNode", "");
        }
    }
}
