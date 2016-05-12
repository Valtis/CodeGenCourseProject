using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACCloneArray : TACValue
    {
        private readonly TACIdentifier source;
        private readonly TACIdentifier destination;

        public TACIdentifier Source
        {
            get
            {
                return source;
            }
        }

        public TACIdentifier Destination
        {
            get
            {
                return destination;
            }
        }

        public TACCloneArray(TACIdentifier source, TACIdentifier destination) 
            : this(0, 0, source, destination)
        {
        }

        public TACCloneArray(
            int line, int column, TACIdentifier source, TACIdentifier destination) 
            : base(line, column)
        {
            this.source = source;
            this.destination = destination;
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            return obj is TACCloneArray &&
                ((TACCloneArray)obj).Source.Equals(Source) &&
                ((TACCloneArray)obj).Destination.Equals(Destination);
        }

        public override string ToString()
        {
            return Destination.ToString() + " = clone(" + Source.ToString() + ")";
        }
    }
}
