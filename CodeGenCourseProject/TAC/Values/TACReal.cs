namespace CodeGenCourseProject.TAC.Values
{
    public class TACReal : TACValue
    {
        private readonly double value;

        public TACReal(double value) : this(0, 0, value)
        {
        }

        public TACReal(int line, int column, double value) : base(line, column)
        {
            this.value = value;
        }

        public double Value
        {
            get
            {
                return value;
            }
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is TACReal && ((TACReal)obj).value == value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
