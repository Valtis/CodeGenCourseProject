using System;

namespace CodeGenCourseProject.Tokens
{
    public class RealToken : Token
    {
        private readonly double? value;

        public double Value
        {
            get
            {
                return value.Value;
            }
        }

        public RealToken(double value)
        {
            this.value = value;
        }

        public RealToken()
        {
            this.value = null;
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("Real", "" + value);
        }

        public override bool Equals(object obj)
        {
            if (obj is RealToken)
            {
                return Value == ((RealToken)obj).Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
