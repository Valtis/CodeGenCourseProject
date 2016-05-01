using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACArrayIndex : TACValue
    {
        private readonly string name;
        private readonly TACValue index;
        private readonly int id;
        private readonly string type;

        public string Type
        {
            get
            {
                return type;
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public TACValue Index
        {
            get
            {
                return index;
            }
        }

        public TACArrayIndex(string name, TACValue index, string type, int id) : 
            this(0, 0, name, index, type, id)
        {

        }

        public TACArrayIndex(
            int line, int column, string name, TACValue index, string type, int id) : base(line, column)
        {
            this.name = name + "_" + id;
            this.index = index;
            this.type = type;
            this.id = id;
        }

        public override string ToString()
        {
            return Name.ToString() + "[" + Index.ToString() + "]";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TACArrayIndex))
            {
                return false;
            }

            var other = (TACArrayIndex)obj;
            return Name.Equals(other.name) && index.Equals(other.Index) && type == other.type && id == other.id;
        }


        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
