namespace CodeGenCourseProject.TAC.Values
{
    /*
    Abstract base class for three-addres code values
    */
    public abstract class TACValue
    {
        private readonly int line;
        private readonly int column;

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

        public TACValue(int line, int column)
        {
            this.line = line;
            this.column = column;
        }
        public abstract void Accept(TACVisitor visitor);
    }
}
