using System;

namespace CodeGenCourseProject.Tokens
{
    public class IdentifierToken : Token
    {
        private readonly string value;

        public IdentifierToken(string identifier)
        {
            this.value = identifier;
        }

        public IdentifierToken()
        {
            value = "";
        }

        public string Identifier
        {
            get
            {
                return Value;
            }
        }

        public string Value
        {
            get
            {
                return value;
            }
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("identifier", Identifier);
        }

        public override bool Equals(object obj)
        {
            return obj is IdentifierToken && Identifier == ((IdentifierToken)obj).Identifier;
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }
    }
}
