using System;

namespace CodeGenCourseProject.Tokens
{
    public class IntegerToken : Token
    {
        private readonly int? value;

        public int Value
        {
            get
            {
                return value.Value;
            }
        }

        public IntegerToken(int value)
        {
            this.value = value;
        }

        public IntegerToken()
        {
            value = null;
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("Integer", "" + value);
        }

        public override bool Equals(object obj)
        {
            if (obj is IntegerToken)
            {
                return Value == ((IntegerToken)obj).Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}
