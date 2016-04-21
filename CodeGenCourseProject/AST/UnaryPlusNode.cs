using System;

namespace CodeGenCourseProject.AST
{
    public class UnaryPlusNode : ASTNode
    {
        public UnaryPlusNode(int line, int column, ASTNode expression) : base(line, column)
        {
            Children.Add(expression);
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("UnaryPlusNode", "");
        }
    }
}
