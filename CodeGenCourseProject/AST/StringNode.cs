using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class StringNode : ASTNode
    {
        private readonly string value;

        public StringNode(int line, int column, string value) : base(line, column)
        {
            this.value = value;
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("StringNode", value);
        }

        public override bool Equals(object obj)
        {
            return obj is StringNode && value == ((StringNode)obj).value;
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string NodeType()
        {
            return "string";
        }
    }
}
