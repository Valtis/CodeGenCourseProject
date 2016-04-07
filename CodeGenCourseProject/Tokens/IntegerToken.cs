using System;

namespace CodeGenCourseProject.Tokens
{
    public class IntegerToken : Token
    {
        private int value;

        public IntegerToken(int value)
        {
            this.value = value;
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("Integer", "" + value);
        }

        public override bool Equals(object obj)
        {
            if (obj is IntegerToken)
            {
                return value == ((IntegerToken)obj).value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value;
        }
    }
}
