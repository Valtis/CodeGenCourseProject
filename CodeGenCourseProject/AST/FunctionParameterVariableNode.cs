using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class FunctionParameterVariableNode : ASTNode
    {
        private readonly bool isReferenceParameter;

        public FunctionParameterVariableNode(int line, int column, ASTNode name, ASTNode type, bool isReference) : base(line, column)
        {
            Children.Add(name);
            Children.Add(type);
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
            return new Tuple<string, string>("FunctionParameterVariableNode", "" + Name.Value + "," + Type.Value + "," + isReferenceParameter);
        }

        public override bool Equals(object obj)
        {
            return obj is FunctionParameterVariableNode && 
                Name.Value ==  ((FunctionParameterVariableNode)obj).Name.Value &&
                Type.Value == ((FunctionParameterVariableNode)obj).Type.Value &&
                IsReferenceParameter == ((FunctionParameterVariableNode)obj).IsReferenceParameter;
        }
    }
}
