using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class IdentifierNode : ASTNode
    {
        private readonly string value;

        public IdentifierNode(int line, int column, string value) : base(line, column)
        {
            this.value = value;
        }


        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("IdentifierNode", value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is IdentifierNode && value == ((IdentifierNode)obj).value;
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
