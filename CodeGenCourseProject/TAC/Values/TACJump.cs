namespace CodeGenCourseProject.TAC.Values
{
    public class TACJump : TACValue
    {
        private readonly TACLabel label;
        public TACJump(TACValue label) : this(0, 0, label)
        {
            this.label = (TACLabel)label;
        }

        public TACJump(int line, int column, TACValue label) : base(line, column)
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

        public override void Accept(TACVisitor visitor)
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
