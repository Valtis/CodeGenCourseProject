using System;

namespace CodeGenCourseProject.AST
{
    public class ArraySizeNode : ASTNode
    {

        public ArraySizeNode(int line, int column, ASTNode array) : base(line, column)
        {
            Children.Add(array);
        }

        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("ArraySizeNode", "");
        }
    }
}
