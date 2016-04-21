using System;

namespace CodeGenCourseProject.AST
{
    public class RealNode : ASTNode
    {
        private readonly double value;

        public double Value
        {
            get
            {
                return value;
            }
        }

        public RealNode(int line, int column, double value) : base(line, column)
        {
            this.value = value;
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("RealNode", Value.ToString());
        }


        public override bool Equals(object obj)
        {
            return obj is RealNode && Value == ((RealNode)obj).Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string NodeType()
        {
            return "real";
        }
    }
}
