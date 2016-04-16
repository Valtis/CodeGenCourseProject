using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class AssertNode : ASTNode
    {
        public AssertNode(int line, int column, ASTNode expression) : base(line, column)
        {
            Children.Add(expression);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("AssertNode", "");
        }
    }
}
