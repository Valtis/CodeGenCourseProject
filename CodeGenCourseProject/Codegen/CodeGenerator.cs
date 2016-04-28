using System.IO;

namespace CodeGenCourseProject.Codegen
{
    public interface CodeGenerator
    {
        void GenerateCode();
        void SaveResult(Stream output);    
    }
}
