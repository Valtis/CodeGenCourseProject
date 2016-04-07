using System;

namespace CodeGenCourseProject.Tokens
{
    public class IdentifierToken : Token
    {
        private readonly string identifier;
        public IdentifierToken(string identifier)
        {
            this.identifier = identifier;
        }

        public string Identifier
        {
            get
            {
                return identifier;
            }
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("identifier", identifier);
        }

        public override bool Equals(object obj)
        {
            return obj is IdentifierToken && identifier == ((IdentifierToken)obj).identifier;
        }

        public override int GetHashCode()
        {
            return identifier.GetHashCode();
        }
    }
}
