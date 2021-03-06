﻿using System;
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
        void Visit(TACReal tacReal);
        void Visit(TACBoolean tacBoolean);
        void Visit(TACString tacString);
    }
}
