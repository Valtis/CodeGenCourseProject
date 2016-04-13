using System;

namespace CodeGenCourseProject.Tokens
{
    public class StringToken : Token
    {
        private readonly string value;

        public string Value
        {
            get
            {
                return value;
            }
        }

        public StringToken(string value)
        {
            this.value = value;
        }

        public StringToken()
        {
            value = null;
        }
             
        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("string", "" + value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is StringToken && Value == ((StringToken)obj).Value;
        }
    }
}
