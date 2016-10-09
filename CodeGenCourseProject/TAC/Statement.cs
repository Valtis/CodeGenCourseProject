using CodeGenCourseProject.TAC.Values;
using System;

namespace CodeGenCourseProject.TAC
{
    public class Statement
    {
        private readonly Tuple<Operator?, TACValue, TACValue, TACValue> quad;

        public Tuple<Operator?, TACValue, TACValue, TACValue> Quad
        {
            get
            {
                return quad;
            }
        }
        
        public int Line
        {
            get
            {
                if (Destination != null)
                {
                    return Destination.Line;
                }

                return RightOperand.Line;
            }
        }

        public int Column
        {
            get
            {
                if (Destination != null)
                {
                    return Destination.Column;
                }

                return RightOperand.Column;
            }
        }

        public Operator? Operator
        {
            get
            {
                return quad.Item1;
            }
        }


        public TACValue LeftOperand
        {
            get
            {
                return quad.Item2;
            }
        }


        public TACValue RightOperand
        {
            get
            {
                return quad.Item3;
            }
        }


        public TACValue Destination
        {
            get
            {
                return quad.Item4;
            }
        }

        public Statement(Operator? op, TACValue lhs, TACValue rhs, TACValue destination)
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
            if (!(obj is Statement))
            {
                return false;
            }

            var other = (Statement)obj;

            return Quad.Equals(other.Quad);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + Quad.Item1.GetHashCode();
            hash = hash * 31 + Quad.Item2.GetHashCode();
            hash = hash * 31 + Quad.Item3.GetHashCode();
            hash = hash * 31 + Quad.Item4.GetHashCode();
            return hash;
        }
    }
}
