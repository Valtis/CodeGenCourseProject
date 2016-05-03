using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACCall : TACValue
    {
        private readonly IList<TACValue> arguments;
        private readonly string function;

        public TACCall(string function, IList<TACValue> arguments) : this(0, 0, function, arguments)
        {
        }

        public TACCall(int line, int column, string function, IList<TACValue> arguments) : base(line, column)
        {
            this.arguments = arguments;
            this.function = function;
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
            return (obj is TACCall) && ((TACCall)obj).arguments.SequenceEqual(arguments);
        }

        public override string ToString()
        {
            return function + "(" + string.Join(", ", arguments) + ")";
        }
    }
}
