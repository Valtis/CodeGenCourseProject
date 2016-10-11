using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.ErrorHandling
{
    // NOTE_GENERIC does not print the 'at <line> <column>' part of the message. Used for generic notes, not related 
    // to any particular line of code
    public enum MessageKind { NOTE, WARNING, LEXICAL_ERROR, SYNTAX_ERROR,
        SEMANTIC_ERROR, NOTE_GENERIC,
    }

    /*
    Message reporter class. Allows reporting various errors, warnings and notes
    */
    public class MessageReporter
    {
        private string[] lines;
        private IList<MessageData> messages;

        public IList<MessageData> Errors
        {
            get
            {
                // only select errors
                var filteredList = from e in messages
                                    where (e.Type == MessageKind.LEXICAL_ERROR || e.Type == MessageKind.SYNTAX_ERROR || e.Type == MessageKind.SEMANTIC_ERROR)
                                    select e;
                return new List<MessageData>(filteredList);
            }
        }

        public IList<MessageData> Warnings
        {
            get
            {
                var filteredList = from e in messages where e.Type == MessageKind.WARNING select e;
                return new List<MessageData>(filteredList);
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

        public MessageReporter()
        {
            messages = new List<MessageData>();
        }

        public void ReportError(MessageKind type, string msg, int line, int column)
        { 
            messages.Add(new MessageData(Lines, type, msg, line, column));
            // errors might get reported out-of-order as far as line/column numbers are considered
            // so sort the error messages after adding a new one.
            messages = messages.OrderBy(m => m.Column).ToList();
            messages = messages.OrderBy(m => m.Line).ToList();
        }

        public void PrintMessages()
        {
            foreach (var error in messages)
            {
                error.Print();
            }
        }

    }
}
