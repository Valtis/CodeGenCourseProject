using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACLabel : TACValue
    {
        private readonly int id;

        public TACLabel(int id) : this(0, 0, id)
        {
        }

        public TACLabel(int line, int column, int id) : base(line, column)
        {
            this.id = id;
        }

        public int ID
        {
            get
            {
                return id;
            }
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            return (obj is TACLabel) && ((TACLabel)obj).id == id;
        }

        public override string ToString()
        {
            return "label_" + id;
        }
    }
}
