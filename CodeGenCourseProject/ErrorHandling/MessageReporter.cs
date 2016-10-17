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

        // errors/warnings & notes are in different lists, as errors might get
        // reported out-of-line-order. Errors are then sorted before reporting them
        // to ensure that errors are in logical order. Notes need to remain attached 
        // to the correct error messages, but their line numbers may point to completely 
        // different sections of code, so naive sorting by column/line will break the ordering
        private IList<MessageData> errorsAndWarnings;
        // error - list of notes
        private IDictionary<MessageData, IList<MessageData>> notes;
        private bool needSorting;

        public MessageReporter()
        {
            errorsAndWarnings = new List<MessageData>();
            notes = new Dictionary<MessageData, IList<MessageData>>();
            needSorting = false;
        }

        public IList<MessageData> Errors
        {
            get
            {
                // only select errors
                var filteredList = from e in errorsAndWarnings
                                    where (e.Type == MessageKind.LEXICAL_ERROR || e.Type == MessageKind.SYNTAX_ERROR || e.Type == MessageKind.SEMANTIC_ERROR)
                                    select e;
                
                var list = new List<MessageData>(filteredList);

                list = list.OrderBy(m => m.Column).ToList();
                list = list.OrderBy(m => m.Line).ToList();
                return list;
            }
        }

        public IList<MessageData> Warnings
        {
            get
            {
                var filteredList = from e in errorsAndWarnings where e.Type == MessageKind.WARNING select e;

                var list = new List<MessageData>(filteredList);
                list = list.OrderBy(m => m.Column).ToList();
                list = list.OrderBy(m => m.Line).ToList();
                return list;
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

        public IList<MessageData> AllMessages
        {
            get
            {
                // new list so that this is equivalent to Errors and Warnings properties 
                // in that modifying the returned list does not modify the internal list
                return GetCombinedMessages();
            }
        }

        public void ReportError(MessageKind type, string msg, int line, int column)
        {
            var message = new MessageData(Lines, type, msg, line, column);

            if (type == MessageKind.NOTE || type == MessageKind.NOTE_GENERIC)
            {
                var attachPoint = errorsAndWarnings[errorsAndWarnings.Count - 1];
                if (!notes.ContainsKey(attachPoint))
                {
                    notes[attachPoint] = new List<MessageData>();
                }
                notes[attachPoint].Add(message);
            }
            else
            {
                errorsAndWarnings.Add(message);
            }
        }

        public void PrintMessages()
        {
            var allMesssages = GetCombinedMessages();
            foreach (var msg in allMesssages)
            {
                msg.Print();
            }
        }

        private IList<MessageData> GetCombinedMessages()
        {
            var list = new List<MessageData>();
            var sortedErrorsAndWarnings = errorsAndWarnings.OrderBy(m => m.Column).ToList();
            sortedErrorsAndWarnings = sortedErrorsAndWarnings.OrderBy(m => m.Line).ToList();

            foreach (var error in sortedErrorsAndWarnings)
            {
                list.Add(error);
                if (notes.ContainsKey(error))
                {
                    foreach (var note in notes[error])
                    {
                        list.Add(note);
                    }
                }
            }
            return list;
        }        
    }
}
