namespace CodeGenCourseProject.TAC.Values
{
    public class TACReal : TACValue
    {
        private readonly double value;

        public TACReal(double value)
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

        public void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
