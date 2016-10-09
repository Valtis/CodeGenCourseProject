using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenCourseProject.TAC
{
    public class Helper
    {
        public static string MangleVariableName(string name, int id)
        {
            return name + "__ID__" + id;
        }

        public static string MangleFunctionName(string name, int id)
        {
            return "__" + name + "__ID__" + id + "__";
        }
    }
}
