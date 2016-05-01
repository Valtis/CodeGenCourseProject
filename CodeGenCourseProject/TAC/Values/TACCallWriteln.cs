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

        public TACCallWriteln(IList<TACValue> arguments) : this(0, 0, arguments)
        {
        }

        public TACCallWriteln(int line, int column, IList<TACValue> arguments) : base(line, column)
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

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            return (obj is TACCallWriteln) && ((TACCallWriteln)obj).arguments.SequenceEqual(arguments);
        }

        public override string ToString()
        {
            return "writeln(" + string.Join(", ", arguments) + ")";
        }
    }
}
