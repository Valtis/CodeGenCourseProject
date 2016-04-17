using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class IfNode : ASTNode
    {
        public IfNode(int line, int column, ASTNode expression, ASTNode ifBody) : base(line, column)
        {
            Children.Add(expression);
            Children.Add(ifBody);
        }

        public IfNode(int line, int column, ASTNode expression, ASTNode ifBody, ASTNode elseBody) : base(line, column)
        {
            Children.Add(expression);
            Children.Add(ifBody);
            Children.Add(elseBody);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("IfNode", "");
        }
    }
}
