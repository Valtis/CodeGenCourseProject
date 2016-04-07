using System;

namespace CodeGenCourseProject
{
    public class InternalCompilerError : Exception
    {
        public InternalCompilerError(string msg) : base(msg)
        {

        }
    }
}
