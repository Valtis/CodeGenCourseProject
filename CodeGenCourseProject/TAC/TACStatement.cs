using CodeGenCourseProject.TAC.Values;
using System;

namespace CodeGenCourseProject.TAC
{
    public class TACStatement
    {
        private readonly Tuple<Operator?, TACValue, TACValue, TACValue> quad;

        public Tuple<Operator?, TACValue, TACValue, TACValue> Quad
        {
            get
            {
                return quad;
            }
        }

        public TACStatement(Operator? op, TACValue lhs, TACValue rhs, TACValue destination)
        {
            quad = new Tuple<Operator?, TACValue, TACValue, TACValue>(op, lhs, rhs, destination);
        }

        public override string ToString()
        {
            var lhs = Quad.Item2 == null ? "" : Quad.Item2.ToString() + " ";
            var op = Quad.Item1.HasValue ? Quad.Item1.Value.Name() : "";
            var rhs = Quad.Item2 == null ? Quad.Item3.ToString() : " " + Quad.Item3.ToString();
            var assign = Quad.Item4?.ToString();

            var ret = "";
            if (assign != null)
            {
                ret = assign + " = ";
            }
            return ret + lhs + op + rhs;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TACStatement))
            {
                return false;
            }

            var other = (TACStatement)obj;

            return Quad.Equals(other.Quad);
        }
    }
}
