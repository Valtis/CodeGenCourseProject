namespace CodeGenCourseProject.TAC.Values
{
    public interface TACValue
    {
        void Accept(TACVisitor visitor);
    }
}
