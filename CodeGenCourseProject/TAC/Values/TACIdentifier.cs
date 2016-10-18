using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{

    public enum AddressingMode { NONE, DEREFERENCE, TAKE_ADDRESS };

    public class TACIdentifier : TACValue
    {
        private readonly string name;
        private readonly string unmangledName;
        private readonly int id;
        private readonly string type;
        private AddressingMode addressingMode;
        private bool isArray;
        private bool isPointer;
        private bool isCaptured;

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

        internal AddressingMode AddressingMode
        {
            get
            {
                return addressingMode;
            }

            set
            {
                addressingMode = value;
            }
        }

        public bool IsPointer
        {
            get
            {
                return isPointer;
            }

            set
            {
                isPointer = value;
            }
        }

        public bool IsCaptured
        {
            get
            {
                return isCaptured;
            }

            set
            {
                isCaptured = value;
            }
        }

        public TACIdentifier(string name, string type, int id, AddressingMode mode = AddressingMode.NONE) : this(0, 0, name, type, id, mode)
        {
        }

        public TACIdentifier(int line, int column, string name, string type, int id, AddressingMode mode = AddressingMode.NONE) : base(line, column)
        {
            this.name = Helper.MangleVariableName(name, id);
            this.unmangledName = name;
            this.type = type;
            this.id = id;
            this.addressingMode = mode;
        }

        public TACIdentifier(TACIdentifier id) : this(id.Line, id.Column, id.UnmangledName, id.Type, id.Id, id.addressingMode)
        {
            this.isArray = id.isArray;
            this.isPointer = id.isPointer;
            this.IsCaptured = id.IsCaptured;
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
