using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACIdentifier : TACValue
    {
        private readonly string name;
        private readonly int id;
        private readonly string type;

        public string Type
        {
            get
            {
                return type;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public TACIdentifier(string name, string type, int id)
        {
            this.name = name + "_" + id;
            this.type = type;
            this.id = id;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TACIdentifier))
            {
                return false;
            }

            var other = (TACIdentifier)obj;
            return name == other.name && type == other.type && id == other.id;
        }


        public void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
