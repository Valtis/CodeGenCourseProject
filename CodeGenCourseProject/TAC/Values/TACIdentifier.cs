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
        private readonly string unmangledName;
        private readonly int id;
        private readonly string type;
        private bool isReference;
        private bool isArray;

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

        public string UnmangledName
        {
            get
            {
                return unmangledName;
            }
        }

        public bool IsReference
        {
            get
            {
                return isReference;
            }

            set
            {
                isReference = value;
            }
        }

        public bool IsArray
        {
            get
            {
                return isArray;
            }

            set
            {
                isArray = value;
            }
        }

        public TACIdentifier(string name, string type, int id, bool isReference = false) : this(0, 0, name, type, id, isReference)
        {
        }

        public TACIdentifier(int line, int column, string name, string type, int id, bool isReference = false) : base(line, column)
        {
            this.name = Helper.MangleVariableName(name, id);
            this.unmangledName = name;
            this.type = type;
            this.id = id;
            this.isReference = isReference;
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

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
