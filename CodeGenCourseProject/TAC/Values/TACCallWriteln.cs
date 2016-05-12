using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACCallWriteln : TACCall
    {
        private readonly IList<TACValue> arguments;

        public TACCallWriteln(IList<TACValue> arguments) : this(0, 0, arguments)
        {
        }

        public TACCallWriteln(int line, int column, IList<TACValue> arguments) : base(line, column, "__inbuilt_writeln", arguments)
        {
            this.arguments = arguments;
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

    }
}
