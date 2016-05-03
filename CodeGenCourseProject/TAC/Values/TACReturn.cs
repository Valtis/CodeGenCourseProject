using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACReturn : TACValue
    {
        public TACReturn() : this(0, 0)
        { }

        public TACReturn(int line, int column) : base(line, column)
        {

        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            return obj is TACReturn;
        }

        public override string ToString()
        {
            return "return";
        }
    }
}
