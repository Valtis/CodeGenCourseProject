namespace CodeGenCourseProject.TAC.Values
{
    public class TACBoolean : TACValue
    {
        private readonly bool value;

        public TACBoolean(bool value)
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

        public void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
