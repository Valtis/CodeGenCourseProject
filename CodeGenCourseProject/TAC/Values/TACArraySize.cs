using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACArraySize : TACValue
    {
        private readonly TACValue array;

        public TACArraySize(TACValue array) : this(0, 0, array)
        {

        }

        public TACArraySize(int line, int column, TACValue array) : base(line, column)
        {
            this.array = array;
        }
        public TACValue Array
        {
            get
            {
                return array;
            }
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            return obj is TACArraySize && ((TACArraySize)obj).array.Equals(array);
        }

        public override string ToString()
        {
            return array.ToString() + ".size";
        }
    }
}
