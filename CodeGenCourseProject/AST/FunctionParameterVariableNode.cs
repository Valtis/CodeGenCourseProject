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

        private readonly string name;
        private readonly string type;

        public FunctionParameterVariableNode(int line, int column, string name, string type, bool isReference) : base(line, column)
        {
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
            return new Tuple<string, string>("FunctionParameterVariableNode", "" + name + "," + type + "," + isReferenceParameter);
        }

        public override bool Equals(object obj)
        {
            return obj is FunctionParameterVariableNode && 
                Name ==  ((FunctionParameterVariableNode)obj).Name &&
                Type == ((FunctionParameterVariableNode)obj).Type &&
                IsReferenceParameter == ((FunctionParameterVariableNode)obj).IsReferenceParameter;
        }
    }
}
