using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Tokens;

namespace CodeGenCourseProject.Lexing
{
    /*
    Scans and removes comments
    */
    class CommentScanner : TokenScanner
    {

        internal CommentScanner(TextReader reader, ErrorReporter reporter) : base(reader, reporter)
        {

        }

        internal override bool Recognizes(char character)
        {
            return character == '{';
        }

        protected override Token DoScan()
        {
            int startLine = Reader.Line;
            int startColumn = Reader.Column;
            Reader.NextCharacter(); // discard '{'
            if (!Reader.PeekCharacter().HasValue || Reader.PeekCharacter().Value != '*')
            {
                Reporter.ReportError(
                    Error.LEXICAL_ERROR,
                    "Unexpected character '" + Reader.PeekCharacter().Value + "' when '*' was expected",
                    Reader.Line,
                    Reader.Column);
                return new CommentToken();
            }
            Reader.NextCharacter(); // discard '*'

            char? character;
            while (true)
            {
                character = Reader.NextCharacter();
                if (!character.HasValue)
                {
                    ReportUnexpectedEOF(startLine, startColumn);
                    return new CommentToken();
                }

                if (character.HasValue && character == '*')
                {
                    if (!character.HasValue)
                    {
                        ReportUnexpectedEOF(startLine, startColumn);
                        return new CommentToken();
                    }

                    character = Reader.NextCharacter();
                    if (character.HasValue && character.Value == '}')
                    {
                        return new CommentToken();
                    }
                }
            }
        }

        private void ReportUnexpectedEOF(int startLine, int startColumn)
        {
            Reporter.ReportError(
               Error.LEXICAL_ERROR,
               "Unexpected end of file when scanning comment",
               Reader.Line-1,
               Reader.Lines[Reader.Line -1].Length-1);

            Reporter.ReportError(
               Error.NOTE,
               "Comment started here",
               startLine,
               startColumn);
        }
    }
}
