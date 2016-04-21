using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public abstract class ASTNode
    {
        private readonly IList<ASTNode> children;
        private readonly int line;
        private readonly int column;
        private string type;

        public IList<ASTNode> Children
        {
            get
            {
                return children;
            }
        }

        public int Line
        {
            get
            {
                return line;
            }
        }

        public int Column
        {
            get
            {
                return column;
            }
        }

        public ASTNode(int line, int column)
        {
            children = new List<ASTNode>();
            this.line = line;
            this.column = column;
        }

        public override string ToString()
        {
            var repr = GetStringRepresentation();
            string ret = "<" + repr.Item1;
            if (repr.Item2.Length != 0)
            {
                ret += " - '" + repr.Item2 + "'";
            }
            return ret + ">";
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj.GetType() == GetType();
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        protected abstract Tuple<String, String> GetStringRepresentation();
        public abstract void Accept(ASTVisitor visitor);

        public virtual string NodeType()
        {
            if (type == "" || type == null)
            {
                throw new InternalCompilerError("Node has no type");
            }
            return type;
        }

        public virtual void SetNodeType(string type)
        {
            this.type = type;
        }
    }
}
