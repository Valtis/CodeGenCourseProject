using System;

namespace CodeGenCourseProject.Tokens
{
    public class StringToken : Token
    {
        private string value;

        public StringToken(string value)
        {
            this.value = value;
        }
             
        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("string", value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is StringToken && value == ((StringToken)obj).value;
        }
    }
}
