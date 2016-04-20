using System;

namespace CodeGenCourseProject.AST
{
    public class VariableDeclarationNode : ASTNode
    {
        public VariableDeclarationNode(int line, int column, ASTNode type, params ASTNode[] names) : base(line, column)
        {
            Children.Add(type);
            foreach (var node in names)
            {
                Children.Add(node);
            }
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("VariableDeclarationNOde", "");
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
