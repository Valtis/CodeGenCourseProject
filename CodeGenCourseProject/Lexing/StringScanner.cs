using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Tokens;
using System.Collections.Generic;
using System.Text;

namespace CodeGenCourseProject.Lexing
{
    /*
    Scan strings
    */
    internal class StringScanner : TokenScanner
    {
        private IDictionary<char, char> escapeSequences;
        internal StringScanner(TextReader reader, ErrorReporter reporter) : base(reader, reporter)
        {
            escapeSequences = new Dictionary<char, char>();
            escapeSequences['n'] = '\n';
            escapeSequences['t'] = '\t';
            escapeSequences['r'] = '\r';
            escapeSequences['b'] = '\b';
            // \e is non-standard escape sequence character, supported by GCC and clang tcc
            // it is accepted for the sole purpose of supporting VT100 escape sequences for the
            // example adventure.pas program
            // Alas, as it is non-standard, we may not actually use \e here as C# compiler rejects it
            // so we have to use the numerical value instead
            escapeSequences['e'] = (char)0x1B;
            escapeSequences['\\'] = '\\';
            escapeSequences['"'] = '\"';
        }

        internal override bool Recognizes(char character)
        {
            return character == '"';
        }
        
        protected override Token DoScan()
        {
              var builder = new StringBuilder();

              // skip first "
              Reader.NextCharacter();
              while (Reader.PeekCharacter().HasValue)
              {
                  var character = Reader.PeekCharacter().Value;
                  if (character == '"')
                  {
                      // discard "
                      Reader.NextCharacter();
                      break;
                  }
                  else if (character == '\n')
                  {
                      Reporter.ReportError(Error.LEXICAL_ERROR,
                          "String is not terminated",
                          Reader.Line,
                          Reader.Column
                          );
                      Reader.NextCharacter();
                      break;
                  }
                  else if (character == '\\')
                  {
                      builder.Append(HandleEscapeSequence());
                  }
                  else
                  {
                      builder.Append(character);
                  }

                  Reader.NextCharacter();
              }

              return new StringToken(builder.ToString());
        }


		private char HandleEscapeSequence()
        {
            // discard '\'
            Reader.NextCharacter();
			// there should be, at very least, a newline character remaining,
			// so getting the value without a check should not cause issues
            var nextChar = Reader.PeekCharacter().Value;

            if (escapeSequences.ContainsKey(nextChar))
            {
                return escapeSequences[nextChar];
            }
            else
            {
                Reporter.ReportError(
                    Error.LEXICAL_ERROR,
                    "Invalid escape sequence '\\" + nextChar + "'",
                    Reader.Line,
                    Reader.Column);
                return nextChar;
            }            
        }
    }
}
