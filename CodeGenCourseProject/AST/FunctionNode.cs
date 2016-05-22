using System;

namespace CodeGenCourseProject.AST
{
    public class FunctionNode : ASTNode
    {
        public FunctionNode(
            int line, 
            int column, 
            IdentifierNode name, 
            ASTNode returnType, 
            ASTNode block, 
            params ASTNode[] parameters) : base(line, column)
        {
            Children.Add(name);
            Children.Add(returnType);
            Children.Add(block);
            foreach (var param in parameters)
            {
                Children.Add(param);
            }
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("FunctionNode", "");
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
