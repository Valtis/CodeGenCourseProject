using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACInteger : TACValue
    {
        private readonly int value;

        public TACInteger(int value)
        {
            this.value = value;
        }

        public int Value
        {
            get
            {
                return value;
            }
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is TACInteger && ((TACInteger)obj).value == value;
        }

        public void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
