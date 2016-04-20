using System;
using CodeGenCourseProject.Tokens;

namespace CodeGenCourseProject.AST
{
    public class ArrayIndexNode : ASTNode
    {
        private IdentifierToken value;

        public ArrayIndexNode(int line, int column, IdentifierToken array, ASTNode index) : base(line, column)
        {
            this.value = array;
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("ArrayIndexNode", value.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayIndexNode && value.Equals(((ArrayIndexNode)obj).value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
