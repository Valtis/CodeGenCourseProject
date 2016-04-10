using System;
using CodeGenCourseProject.Tokens;

namespace CodeGenCourseProject.AST
{
    public class ArrayIndex : ASTNode
    {
        private IdentifierToken value;

        public ArrayIndex(int line, int column, IdentifierToken array, ASTNode index) : base(line, column)
        {
            this.value = array;
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("ArrayIndexNode", "");
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayIndex && value.Equals(((ArrayIndex)obj).value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

    }
}
