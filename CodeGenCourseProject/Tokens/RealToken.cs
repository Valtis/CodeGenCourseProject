using System;

namespace CodeGenCourseProject.Tokens
{
    public class RealToken : Token
    {
        private double value;

        public RealToken(double value)
        {
            this.value = value;
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("Real", "" + value);
        }

        public override bool Equals(object obj)
        {
            if (obj is RealToken)
            {
                return value == ((RealToken)obj).value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}
