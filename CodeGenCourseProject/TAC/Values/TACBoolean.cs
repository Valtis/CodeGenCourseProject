namespace CodeGenCourseProject.TAC.Values
{
    public class TACBoolean : TACValue
    {
        private readonly bool value;

        public TACBoolean(bool value) : this(0, 0, value)
        {
        }

        public TACBoolean(int line, int column, bool value) : base(line, column)
        {
            this.value = value;
        }

        public bool Value
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
            return obj is TACBoolean && ((TACBoolean)obj).value == value;
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
