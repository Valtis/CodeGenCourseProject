namespace CodeGenCourseProject.TAC.Values
{
    /*
    Abstract base class for three-addres code values
    */
    public abstract class TACValue
    {
        private int line;
        private int column;

        public int Line
        {
            get
            {
                return line;
            }
            set
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
            set
            {
                column = value;
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
