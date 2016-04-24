using System;

namespace CodeGenCourseProject.AST
{
    public class FunctionParameterArrayNode : ASTNode
    {
        private readonly bool isReferenceParameter;

        private readonly string name;
        private readonly string type;

        public FunctionParameterArrayNode(int line, int column, ASTNode expr, string name, string type, bool isReference) : base(line, column)
        {
            if (expr != null)
            {
                Children.Add(expr);
            }
            this.name = name;
            this.type = type;
            this.isReferenceParameter = isReference;
        }

        public bool IsReferenceParameter
        {
            get
            {
                return isReferenceParameter;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("FunctionParameterArrayNode", "" + name + "," + type + "," + isReferenceParameter);
        }

        public override bool Equals(object obj)
        {
            return obj is FunctionParameterArrayNode &&
                Name == ((FunctionParameterArrayNode)obj).Name &&
                Type == ((FunctionParameterArrayNode)obj).Type &&
                IsReferenceParameter == ((FunctionParameterArrayNode)obj).IsReferenceParameter;
        }
    }
}
