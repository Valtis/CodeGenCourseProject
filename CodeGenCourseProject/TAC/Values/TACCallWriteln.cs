using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACCallWriteln : TACValue
    {
        private readonly IList<TACValue> arguments;

        public TACCallWriteln(IList<TACValue> arguments)
        {
            this.arguments = arguments;
        }

        public IList<TACValue> Arguments
        {
            get
            {
                return arguments;
            }
        }

        public void Accept(TACVisitor visitor)
        {
            visitor.Accept(this);
        }

        public override string ToString()
        {
            return "writeln(" + string.Join(", ", arguments) + ")";
        }
    }
}
