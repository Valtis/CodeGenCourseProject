using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACFunctionIdentifier : TACValue
    {
        private readonly string name;
        private readonly string unmangledName;
        private readonly int id;

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

        public TACFunctionIdentifier(string name, int id) : base(0, 0)
        {
            this.name = Helper.MangleFunctionName(name, id);
            this.unmangledName = name;
            this.id = id;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TACFunctionIdentifier))
            {
                return false;
            }

            var other = (TACFunctionIdentifier)obj;
            return name == other.name && id == other.id;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override void Accept(TACVisitor visitor)
        {
            throw new NotImplementedException("This should not be called");
        }
    }
}
