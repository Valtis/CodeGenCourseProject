using System;

namespace CodeGenCourseProject.AST
{
    public class FunctionParameterArrayNode : ASTNode
    {
        private readonly bool isReferenceParameter;

        public FunctionParameterArrayNode(int line, int column, ASTNode expr, ASTNode name, ASTNode type, bool isReference) : base(line, column)
        {
            Children.Add(name);
            Children.Add(type);
            if (expr != null)
            {
                Children.Add(expr);
            }
            this.isReferenceParameter = isReference;
        }

        public bool IsReferenceParameter
        {
            get
            {
                return isReferenceParameter;
            }
        }

        public IdentifierNode Name
        {
            get
            {
                return (IdentifierNode)Children[0];
            }
        }

        public IdentifierNode Type
        {
            get
            {
                return (IdentifierNode)Children[1];
            }
        }

        public override void Accept(ASTVisitor visitor)
        {
            visitor.Visit(this);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("FunctionParameterArrayNode", "" + Name.Value + "," + Type.Value + "," + isReferenceParameter);
        }

        public override bool Equals(object obj)
        {
            return obj is FunctionParameterArrayNode &&
                Name.Value == ((FunctionParameterArrayNode)obj).Name.Value &&
                Type.Value == ((FunctionParameterArrayNode)obj).Type.Value &&
                IsReferenceParameter == ((FunctionParameterArrayNode)obj).IsReferenceParameter;
        }
    }
}
