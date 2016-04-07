using System;

namespace CodeGenCourseProject.Tokens
{
    public class EOFToken : Token
    {
        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string> ( "EOF", "End of file" );
        }
    }
}
