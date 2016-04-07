using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.ErrorHandling
{
    // NOTE_GENERIC does not print the 'at <line> <column>' part of the message. Used for generic notes, not related 
    // to any particular line of code
    public enum Error { NOTE, WARNING, LEXICAL_ERROR, SYNTAX_ERROR,
        SEMANTIC_ERROR, NOTE_GENERIC,
    }

    /*
    Error reporter class. Allows reporting various errors, warnings and notes
    */
    public class ErrorReporter
    {
        private string[] lines;
        private IList<ErrorData> errors;

        public IList<ErrorData> Errors
        {
            get
            {
                // only select errors
                var filteredList = from e in errors
                                    where (e.Type == Error.LEXICAL_ERROR || e.Type == Error.SYNTAX_ERROR || e.Type == Error.SEMANTIC_ERROR)
                                    select e;
                return new List<ErrorData>(filteredList);
            }
        }

        public IList<ErrorData> Warnings
        {
            get
            {
                var filteredList = from e in errors where e.Type == Error.WARNING select e;
                return new List<ErrorData>(filteredList);
            }
        }

        public string[] Lines
        {
            get
            {
                return lines;
            }

            set
            {
                lines = value;
            }
        }

        public ErrorReporter()
        {
            errors = new List<ErrorData>();
        }

        public void ReportError(Error type, string msg, int line, int column)
        {
            errors.Add(new ErrorData(Lines, type, msg, line, column));
        }   

        public void PrintMessages()
        {
            foreach (var error in errors)
            {
                error.Print();
            }
        }

    }
}
