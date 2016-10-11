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
        public class VariableInitPoint
        {
            public readonly TACIdentifier identifier;
            public readonly int initPoint;

            public VariableInitPoint(TACIdentifier identifier, int initPoint)
            {
                this.identifier = identifier;
                this.initPoint = initPoint;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is VariableInitPoint) || obj == null)
                {
                    return false;
                }

                var other = obj as VariableInitPoint;
                return other.identifier.Equals(identifier);
            }

            public override int GetHashCode()
            {
                return identifier.GetHashCode();
            }
        }
        
        private readonly int id;
        private int start;
        private int end;
        private ISet<VariableInitPoint> variableInitializations;
        private ISet<VariableInitPoint> parentInitializations;

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

        public ISet<VariableInitPoint> VariableInitializations
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

        public ISet<VariableInitPoint> ParentInitializations
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

            VariableInitializations = new HashSet<VariableInitPoint>();
            ParentInitializations = new HashSet<VariableInitPoint>();
        }

        internal void MoveUpwards(int length)
        {
            start -= length;
            end -= length;
        }
    }
}
