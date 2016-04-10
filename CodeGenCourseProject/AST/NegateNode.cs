using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class NegateNode : ASTNode
    {
        public NegateNode(int line, int column, ASTNode value) : base(line, column)
        {
            Children.Add(value);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("NegateNode", "");
        }
    }
}
