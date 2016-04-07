using System;

namespace CodeGenCourseProject.Tokens
{
    public class WhitespaceToken : Token
    {
        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("whitespace", "");
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.GetType().Equals(this.GetType());
        }

        public override int GetHashCode()
        {
            // all keyword-tokens are considered to be equal (line\column are ignored), 
            // so return a static hashcode. In general, these should not be used as
            // keys for hashsets\dictionaries etc.
            return 0;
        }
    }
}
