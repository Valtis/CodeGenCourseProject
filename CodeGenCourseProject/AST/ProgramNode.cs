using CodeGenCourseProject.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class ProgramNode : ASTNode
    {
        private IdentifierToken identifier;
        
        public ProgramNode(int line, int column, IdentifierToken identifier, ASTNode programBlock) : base(line, column)
        {
            this.identifier = identifier;
            Children.Add(programBlock);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("ProgramNode", identifier.Identifier);
        }

        public override bool Equals(object obj)
        {
            return obj is ProgramNode && ((ProgramNode)obj).identifier.Identifier == identifier.Identifier;
        }

    }
}
