using CodeGenCourseProject.Tokens;
using System;

namespace CodeGenCourseProject.AST
{
    public class VariableAssignmentNode : ASTNode
    {
        private IdentifierToken identifier;

        public VariableAssignmentNode(
            int line, 
            int column, 
            IdentifierToken variable, 
            ASTNode expression) : base(line, column)
        {
            this.identifier = variable;
            this.Children.Add(expression);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("AssignmentNode", identifier.Identifier);
        }

        public override bool Equals(object obj)
        {
            return obj is VariableAssignmentNode && 
                ((VariableAssignmentNode)obj).identifier.Identifier == identifier.Identifier;
        }
    }
}
