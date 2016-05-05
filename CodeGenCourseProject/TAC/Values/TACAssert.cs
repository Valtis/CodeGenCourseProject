using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACAssert : TACValue
    {
        private readonly TACValue expression;
        public TACAssert(int line, int column, TACValue expression) : base(line, column)
        {
            this.expression = expression;
        }

        public TACAssert(TACValue expression) : this(0, 0, expression)
        {

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
            return obj is TACAssert && expression.Equals(((TACAssert)obj).expression);
        }

        public override string ToString()
        {
            return "assert(" + expression.ToString() + ")";
        }
    }
}
