using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACJumpIfFalse : TACValue
    {
        private readonly TACValue condition;
        private readonly TACLabel label;

        public TACJumpIfFalse(TACValue condition, TACValue label) : this(0, 0, condition, label)
        {
        }

        public TACJumpIfFalse(int line, int column, TACValue condition, TACValue label) : base(line, column)
        {
            this.condition = condition;
            this.label = (TACLabel)label;
        }


        public TACValue Condition
        {
            get
            {
                return condition;
            }
        }

        public TACLabel Label
        {
            get
            {
                return label;
            }
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            return (obj is TACJumpIfFalse) 
                && ((TACJumpIfFalse)obj).label.Equals(label)
                && ((TACJumpIfFalse)obj).condition.Equals(condition);
        }

        public override string ToString()
        {
            return "jump " + label.ToString() + " if not " + condition;
        }
    }
}
