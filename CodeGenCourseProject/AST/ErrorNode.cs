using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class ErrorNode : ASTNode
    {
        public ErrorNode() : base(0, 0)
        {

        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("ErrorNode", "");
        }
    }
}
