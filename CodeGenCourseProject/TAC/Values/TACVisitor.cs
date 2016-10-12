using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenCourseProject.TAC.Values;

namespace CodeGenCourseProject.TAC.Values
{
    public interface TACVisitor
    {
        void Visit(TACInteger tacInteger);
        void Visit(TACIdentifier tacIdentifier);
        void Visit(TACArrayIndex tacArrayIndex);
        void Visit(TACAssert tacAssert);
        void Visit(TACArrayDeclaration tacArrayDeclaration);
        void Visit(TACReal tacReal);
        void Visit(TACCloneArray tacCloneArray);
        void Visit(TACBoolean tacBoolean);
        void Visit(TACArraySize tacArraySize);
        void Visit(TACString tacString);
    }
}
