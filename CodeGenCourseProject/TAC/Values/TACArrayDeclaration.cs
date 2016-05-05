using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC.Values
{
    public class TACArrayDeclaration : TACValue
    {
        private readonly string name;
        private readonly string type;
        private readonly TACValue sizeExpression;
        private readonly int id;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }
        }

        public TACValue Expression
        {
            get
            {
                return sizeExpression;
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public TACArrayDeclaration(string name, string type, TACValue sizeExpression, int id) : 
            this(0, 0, name, type, sizeExpression, id)
        {

        }

        public TACArrayDeclaration(int line, int column,
            string name, string type, TACValue sizeExpression, int id) : base(line, column)
        {
            this.name = name + "_" + id;
            this.type = type;
            this.id = id;
            this.sizeExpression = sizeExpression;
        }

        public override void Accept(TACVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return type + " " + name + "[" + sizeExpression + "]";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TACArrayDeclaration))
            {
                return false;
            }

            var other = (TACArrayDeclaration)obj;

            return name == other.Name && type == other.Type && sizeExpression.Equals(other.sizeExpression)
                && id == other.id;
        }
    }
}
