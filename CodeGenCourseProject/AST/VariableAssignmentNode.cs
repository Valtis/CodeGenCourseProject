using CodeGenCourseProject.Tokens;
using System;

namespace CodeGenCourseProject.AST
{
    public class VariableAssignmentNode : ASTNode
    {
        public VariableAssignmentNode(
            int line, 
            int column, 
            ASTNode variable, 
            ASTNode expression) : base(line, column)
        {
            this.Children.Add(variable);
            this.Children.Add(expression);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("AssignmentNode", "");
        }
    }
}
