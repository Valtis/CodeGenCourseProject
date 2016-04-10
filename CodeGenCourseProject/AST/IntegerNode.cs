using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.AST
{
    public class IntegerNode :  ASTNode
    {
        private readonly int value;

        public int Value
        {
            get
            {
                return value;
            }
        }

        public IntegerNode() : base(0, 0)
        {
            value = 0;
        }

        public IntegerNode(int line, int column, int value) : base(line, column)
        {
            this.value = value;
        }


        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("IntegerNode", Value.ToString());
        }

        public override bool Equals(object obj)
        {
            return obj is IntegerNode && ((IntegerNode)obj).Value == value;
        }
    }
}
