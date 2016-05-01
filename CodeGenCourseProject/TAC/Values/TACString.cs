namespace CodeGenCourseProject.TAC.Values
{
    public class TACString : TACValue
    {
        private readonly string value;

        public TACString(string value) : this(0, 0, value)
        {
        }

        public TACString(int line, int column, string value) : base(line, column)
        {
            this.value = value;
        }

        public string Value
        {
            get
            {
                return value;
            }
        }

        public override string ToString()
        {
            return "\"" +value.ToString() + "\"";
        }

        public override bool Equals(object obj)
        {
            return obj is TACString && ((TACString)obj).value == value;
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
