using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC
{
    public class Function
    {
        private readonly string name;
        private readonly IList<TACStatement> code;

        public Function(string name)
        {
            this.name = name;
            this.code = new List<TACStatement>();
        }

        public IList<TACStatement> Code
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

        public override string ToString()
        {
            return Name;
        }
    }
}
