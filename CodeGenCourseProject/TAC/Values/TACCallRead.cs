using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACCallRead : TACCall
    {
        public TACCallRead(IList<TACValue> arguments) : this(0, 0, arguments)
        { }

        public TACCallRead(int line, int column, IList<TACValue> arguments) : base(line, column, "__inbuilt_read", arguments)
        {

        }
    }
}
