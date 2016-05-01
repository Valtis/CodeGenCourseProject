using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenCourseProject.TAC.Values;

namespace CodeGenCourseProject.CFG
{
    public class BasicBlock
    {
        private readonly int id;
        private readonly int start;
        private readonly int end;
        private ISet<TACIdentifier> variableInitializations;
        private ISet<TACIdentifier> parentInitializations;

        public int ID
        {
            get
            {
                return id;
            }
        }

        public int Start
        {
            get
            {
                return start;
            }
        }

        public int End
        {
            get
            {
                return end;
            }
        }

        public ISet<TACIdentifier> VariableInitializations
        {
            get
            {
                return variableInitializations;
            }

            set
            {
                variableInitializations = value;
            }
        }

        public ISet<TACIdentifier> ParentInitializations
        {
            get
            {
                return parentInitializations;
            }

            set
            {
                parentInitializations = value;
            }
        }

        public BasicBlock(int start, int end, int id)
        {
            this.start = start;
            this.end = end;
            this.id = id;

            VariableInitializations = new HashSet<TACIdentifier>();
            ParentInitializations = new HashSet<TACIdentifier>();
        }
       
    }
}
