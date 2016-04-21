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
        private string type;

        public string Value
        {
            get
            {
                return value;
            }
        }

        public IdentifierNode(int line, int column, string value) : base(line, column)
        {
            this.value = value;
        }


        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("IdentifierNode", Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is IdentifierNode && Value == ((IdentifierNode)obj).Value;
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string NodeType()
        {
            return type;
        }

        public override void SetNodeType(string type)
        {
            this.type = type;
        }
    }
}
