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
        internal class Parameter
        {
            private readonly TACIdentifier identifier;
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

            public Parameter(TACIdentifier id, bool isReference)
            {
                this.identifier = id;
                this.isReference = isReference;
            }
        }

        private readonly string name;

        private readonly IList<Parameter> parameters;

        private readonly IList<TACStatement> code;

        public Function(string name)
        {
            this.name = name;
            this.parameters = new List<Parameter>();
            this.code = new List<TACStatement>();
        }

        public IList<TACStatement> Statements
        {
            get
            {
                return code;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        internal IList<Parameter> Parameters
        {
            get
            {
                return parameters;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        internal void AddParameter(TACIdentifier id, bool isReferenceParameter)
        {
            Parameters.Add(new Parameter(id, isReferenceParameter));
        }
    }
}
