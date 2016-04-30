namespace CodeGenCourseProject.TAC.Values
{
    public class TACJump : TACValue
    {
        private readonly TACLabel label;
        public TACJump(TACValue label)
        {
            this.label = (TACLabel)label;
        }

        public TACLabel Label
        {
            get
            {
                return label;
            }
        }

        public void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override bool Equals(object obj)
        {
            return (obj is TACJump) && ((TACJump)obj).label.Equals(label);
        }

        public override string ToString()
        {
            return "jump " + label;
        }
    }
}
