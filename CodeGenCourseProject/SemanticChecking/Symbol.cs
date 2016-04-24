using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.SemanticChecking
{
    internal abstract class Symbol
    {
        private readonly string name;
        private readonly string type;
        private readonly int line;
        private readonly int column;
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

        public int Id
        {
            get
            {
                return id;
            }
        }

        public int Line
        {
            get
            {
                return line;
            }
        }

        public int Column
        {
            get
            {
                return column;
            }
        }

        public Symbol(int line, int column, string name, string type, int id)
        {
            this.line = line;
            this.column = column;
            this.name = name;
            this.type = type;
            this.id = id;
        }
    }


    internal class VariableSymbol : Symbol
    {
        public VariableSymbol(int line, int column, string name, string type, int id) : base(line, column, name, type, id)
        {
        }
    }

    internal class ArraySymbol : Symbol
    {
        private readonly string baseType;

        public ArraySymbol(int line, int column, string name, string type, int id) : 
            base(line, column, name, "Array<" + type + ">", id)
        {
            baseType = type;
        }

        public string BaseType
        {
            get
            {
                return baseType;
            }
        }
    }
}
