using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACReturn : TACValue
    {
        private readonly TACValue expression;

        public TACReturn() : this(0, 0)
        {

        }

        public TACReturn(int line, int column) : this(line, column, null)
        {

        }

        public TACReturn(TACValue expression) : this(0, 0, expression)
        {

        }

        public TACReturn(int line, int column, TACValue expression) : base(line, column)
        {
            this.expression = expression;
        }

        public TACValue Expression
        {
            get
            {
                return expression;
            }
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TACReturn))
            {
                return false;
            }

            var other = (TACReturn)obj;
            if (expression == null && other.expression == null)
            {
                return true;
            }

            if (expression != null)
            {
                return expression.Equals(other.expression);
            }

            return false;
        }

        public override string ToString()
        {
            return "return " + expression;
        }
    }
}
