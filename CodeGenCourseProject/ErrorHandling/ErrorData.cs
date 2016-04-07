using System;

namespace CodeGenCourseProject.ErrorHandling
{

    /*
    Class representing a single error, warning or note 
    */
    public class ErrorData
    {
        private readonly Error type;
        private readonly string msg;
        private readonly int line;
        private readonly int column;
        private string[] lines;

        public Error Type
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

        public ErrorData(string[] lines, Error type, string msg, int line, int column)
        {
            this.lines = lines;
            this.type = type;
            this.msg = msg;
            this.line = line;
            this.column = column;
        }


        public void Print()
        {
            ConsoleColor color = Console.ForegroundColor;
            switch (Type)
            {
                
                case Error.NOTE_GENERIC:
                case Error.NOTE:
                    color = ConsoleColor.Cyan;
                    Console.ForegroundColor = color;
                    Console.Write("Note: ");
                    break;

                case Error.WARNING:
                    color = ConsoleColor.Yellow;
                    Console.ForegroundColor = color;
                    Console.Write("\n\nWarning: ");
                    break;

                case Error.LEXICAL_ERROR:
                    color = ConsoleColor.Red;
                    Console.ForegroundColor = color;
                    Console.Write("\n\nLexical error: ");
                    break;

                case Error.SEMANTIC_ERROR:
                    color = ConsoleColor.Red;
                    Console.ForegroundColor = color;
                    Console.Write("\n\nSemantic error: ");
                    break;

                case Error.SYNTAX_ERROR:
                    color = ConsoleColor.Red;
                    Console.ForegroundColor = color;
                    Console.Write("\n\nSyntax error: ");
                    break;
                default:
                    Console.Write("<INVALID_ERROR_TYPE>: ");
                    break;
            }


            Console.ResetColor();
            // line, column are zero-based internally
            Console.Write(msg);
            if (type != Error.NOTE_GENERIC)
            {
                Console.Write(" at line " + (line + 1) + " column " + (column + 1));
            }
            if (lines.Length == 0)
            {
                return;
            }

            if (type != Error.NOTE_GENERIC)
            { 
                Console.Write("\n\n" + (line < lines.Length ? lines[line] : ""));

                Console.ForegroundColor = color;
                if (column > 10)
                {
                    for (int i = 0; i < column - 6; ++i)
                    {
                        Console.Write(" ");
                    }

                    for (int i = 0; i < 6; ++i)
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

                    for (int i = 0; i < 6; ++i)
                    {
                        Console.Write("~");
                    }
                }
            }
            Console.Write("\n");
            Console.ResetColor();
        }

    }
}
