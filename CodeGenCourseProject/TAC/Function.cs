using CodeGenCourseProject.TAC.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC
{
    public class Function
    {
        public class Variable
        {
            private readonly TACIdentifier identifier;
            private readonly string type;
            private readonly bool isReference;

            public TACIdentifier Identifier
            {
                get
                {
                    return identifier;
                }
            }

            public bool IsReference
            {
                get
                {
                    return isReference;
                }
            }

            public string Type
            {
                get
                {
                    return type;
                }
            }

            public Variable(TACIdentifier id, string type, bool isReference)
            {
                this.identifier = id;
                this.type = type;
                this.isReference = isReference;
            }

            public override bool Equals(object obj)
            {
                return obj is Variable &&
                    ((Variable)obj).identifier.Equals(identifier);
            }

            public override int GetHashCode()
            {
                return identifier.GetHashCode();
            }
        }

        private readonly int line;
        private readonly int column;
        private readonly string name;
        private readonly string unmangledName;
        private readonly IList<Variable> parameters;

        private readonly ISet<Variable> locals; // local variables
        // values that function captures from outer context
        private ISet<Variable> capturedVariables;
        private IList<TACStatement> statements;
        private readonly string returnType;

        public Function(int line, int column, string name, int id, string returnType)
        {
            this.line = line;
            this.column = column;
            this.parameters = new List<Variable>();
            this.statements = new List<TACStatement>();
            this.CapturedVariables = new HashSet<Variable>();
            this.locals = new HashSet<Variable>();
            this.returnType = returnType;
            if (name != TACGenerator.ENTRY_POINT)
            {
                this.name = Helper.MangleFunctionName(name, id);
                this.unmangledName = name;
            }
            else
            {
                this.name = name;
                this.unmangledName = name;
            }

        }

        public IList<TACStatement> Statements
        {
            get
            {
                return statements;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        internal IList<Variable> Parameters
        {
            get
            {
                return parameters;
            }
        }

        public string ReturnType
        {
            get
            {
                return returnType;
            }
        }

        public string UnmangledName
        {
            get
            {
                return unmangledName;
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

        public ISet<Variable> CapturedVariables
        {
            get
            {
                return capturedVariables;
            }

            set
            {
                capturedVariables = value;
            }
        }

        public ISet<Variable> Locals
        {
            get
            {
                return locals;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        internal void AddParameter(TACIdentifier id, string type, bool isReferenceParameter)
        {
            Parameters.Add(new Variable(id, type, isReferenceParameter));
        }

        internal void UpdateStatements(List<TACStatement> liveStatements)
        {
            statements = liveStatements;
        }
    }
}
