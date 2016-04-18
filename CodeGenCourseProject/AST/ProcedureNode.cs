using System;

namespace CodeGenCourseProject.AST
{
    public class ProcedureNode : ASTNode
    {
        public ProcedureNode(int line, int column, ASTNode identifier, ASTNode block, params ASTNode [] parameters) 
            : base(line, column)
        {
            Children.Add(identifier);
            Children.Add(block);
            foreach (var param in parameters)
            {
                Children.Add(param);
            }
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("ProcedureNode", "");
        }
    }
}
