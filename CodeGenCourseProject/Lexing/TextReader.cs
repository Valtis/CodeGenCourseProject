namespace CodeGenCourseProject.Lexing
{
    /*
    Class abstracting reading characters from a file. Supports backtracking
    */
    internal class TextReader
    {
        // reading and keeping all lines in memory makes implementation easier, but can be expensive
        // memory wise. This is an intentional tradeoff. 
        private string[] lines;

        private int line;

        // column is meant for human-readable notifications, where tab character gets converted
        // into spaces. array_pos tracks the position at given line, used internally
        private int column;
        private int arrayPos;
        
        private int spacesPerTab;

        public int Line
        {
            get
            {
                return line;
            }

            private set
            {
                line = value;
            }
        }

        public int Column
        {
            get
            {
                return column;
            }

            private set
            {
                column = value;
            }
        }

        // backtrack a character. Move back a line if necessary
        internal void Backtrack()
        {
            if (arrayPos != 0)
            {
                --arrayPos;
                --Column;
            }
            else if (Line != 0)
            {
                --Line;
                arrayPos = lines[Line].Length - 1;
            }
        }

        public string[] Lines
        {
            get
            {
                return lines;
            }

        }

        internal TextReader(string path, int spacesPerTab)
        {
            Line = 0;
            Column = 0;
            arrayPos = 0;
            this.spacesPerTab = spacesPerTab;

            lines = System.IO.File.ReadAllLines(path);

            // append newlines to every line, as ReadAllLines strips them but scanners
            // assume they are present
            for (int i = 0; i < Lines.Length; ++i)
            {
                Lines[i] += "\n";
            }
        }

        internal char? NextCharacter()
        {
            var value = PeekCharacter();

            ++arrayPos;

            if (value.HasValue && value.Value == '\t')
            {
                // move to next tab stop
                var distanceFromPreviousStop = Column % spacesPerTab;

                Column += (spacesPerTab - distanceFromPreviousStop);
            }
            else
            {
                Column += 1;
            }
            
            if (Line < Lines.Length && arrayPos >= Lines[Line].Length)
            {
                arrayPos = 0;
                Column = 0;
                ++Line;
                if (Line >= Lines.Length)
                {
                    Line = Lines.Length;
                }
            }

            return value;
        }

        internal char? PeekCharacter()
        {
            if (Line == Lines.Length)
            {
                return null;
            }
            else
            {
                return Lines[Line][arrayPos];
            }
        }
    }
}
