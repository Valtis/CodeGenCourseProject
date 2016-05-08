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
        private readonly bool isReference;

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

        public bool IsReference
        {
            get
            {
                return isReference;
            }
        }

        public TACArrayIndex(string name, TACValue index, string type, int id, bool isReference = false) : 
            this(0, 0, name, index, type, id, isReference)
        {

        }

        public TACArrayIndex(
            int line, int column, string name, TACValue index, string type, int id, bool isReference = false) 
            : base(line, column)
        {
            this.name = Helper.MangleVariableName(name, id);
            this.index = index;
            this.type = type;
            this.id = id;
            this.isReference = isReference;
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
