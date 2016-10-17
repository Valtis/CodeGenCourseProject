using System;

namespace CodeGenCourseProject.ErrorHandling
{

    /*
    Class representing a single error, warning or note 
    */
    public class MessageData
    {
        private readonly MessageKind type;
        private readonly string msg;

        // line, column are zero-based internally
        private readonly int line;
        private readonly int column;
        private string[] lines;


        private const ConsoleColor CODE_LINE_COLOR = ConsoleColor.White;
        private const int DIRECTION_CHANGE_COLUMN_LENGTH = 10;
        private const int LINE_LENGTH = 6;

        public MessageKind Type
        {
            get
            {
                return type;
            }
        }

        public int Line
        {
            get
            {
                return line;
            }
        }

        public int Column
        {
            get
            {
                return column;
            }
        }

        public string Message
        {
            get
            {
                return msg;
            }
        }

        public MessageData(string[] lines, MessageKind type, string msg, int line, int column)
        {
            this.lines = lines;
            this.type = type;
            this.msg = msg;
            this.line = line;
            this.column = column;
        }


        public void Print()
        {
            var color = Console.ForegroundColor;
            switch (Type)
            {

                case MessageKind.NOTE_GENERIC:
                case MessageKind.NOTE:
                    color = ConsoleColor.Cyan;
                    Console.ForegroundColor = color;
                    Console.Write("Note: ");
                    break;

                case MessageKind.WARNING:
                    color = ConsoleColor.Yellow;
                    Console.ForegroundColor = color;
                    Console.Write("\n\nWarning: ");
                    break;

                case MessageKind.LEXICAL_ERROR:
                    color = ConsoleColor.Red;
                    Console.ForegroundColor = color;
                    Console.Write("\n\nLexical error: ");
                    break;

                case MessageKind.SEMANTIC_ERROR:
                    color = ConsoleColor.Red;
                    Console.ForegroundColor = color;
                    Console.Write("\n\nSemantic error: ");
                    break;

                case MessageKind.SYNTAX_ERROR:
                    color = ConsoleColor.Red;
                    Console.ForegroundColor = color;
                    Console.Write("\n\nSyntax error: ");
                    break;
                default:
                    Console.Write("<INVALID_MESSAGE_TYPE>: ");
                    break;
            }


            Console.ResetColor();
            Console.Write(msg);
            
            if (type != MessageKind.NOTE_GENERIC)
            {
                Console.Write(" at line " + (line + 1) + " column " + (column + 1));
            }

            if (lines.Length == 0)
            {
                return;
            }

            if (type != MessageKind.NOTE_GENERIC)
            {
                // Print the lines before and after message, as well as the message line and the ~~~~~ line
                // Also naturally handle the case where the message is on the first or last line, and the
                // previous or following line cannot be printed

                PrintPrecedingCodeLine();
                PrintTheFocusLine();
                PrintHighlightingLine(color);
                PrintTrailingLine();
            }

            Console.ResetColor();
            Console.Write("\n");

        }

        private void PrintPrecedingCodeLine()
        {
            Console.ForegroundColor = CODE_LINE_COLOR;
            if (line > 0)
            {
                Console.Write("\n\n" + (line < lines.Length ? lines[line - 1] : ""));
            }
            else
            {
                Console.Write("\n\n");
            }
        }

        private void PrintTheFocusLine()
        {
            Console.ForegroundColor = CODE_LINE_COLOR;
            Console.Write((line < lines.Length ? lines[line] : ""));
        }              

        private void PrintHighlightingLine(ConsoleColor color)
        {
            Console.ForegroundColor = color;
            if (column > DIRECTION_CHANGE_COLUMN_LENGTH)
            {
                for (int i = 0; i < column - LINE_LENGTH; ++i)
                {
                    Console.Write(" ");
                }

                for (int i = 0; i < LINE_LENGTH; ++i)
                {
                    Console.Write("~");
                }
                Console.Write("^");

            }
            else
            {
                for (int i = 0; i < column; ++i)
                {
                    Console.Write(" ");
                }

                Console.Write("^");

                for (int i = 0; i < LINE_LENGTH; ++i)
                {
                    Console.Write("~");
                }
            }
        }

        private void PrintTrailingLine()
        {
            Console.ForegroundColor = CODE_LINE_COLOR;

            if (line + 1 <= lines.Length - 1)
            {
                Console.Write("\n" + (line < lines.Length ? lines[line + 1] : ""));

            }
        }


        public override bool Equals(object obj)
        {
            MessageData other = obj as MessageData;
            if (other == null)
            {
                return false;
            }

            return line == other.line && column == other.column && msg.Equals(other.Message) && type == other.type;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + msg.GetHashCode();
            hash = hash * 31 + line;
            hash = hash * 31 + column;
            hash = hash * 31 + type.GetHashCode();
            return hash;
        }
    }
}
