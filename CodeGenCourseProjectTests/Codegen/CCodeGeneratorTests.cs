using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.Parsing;
using CodeGenCourseProject.TAC;
using CodeGenCourseProject.CFG;
using CodeGenCourseProject.CFG.Analysis;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace CodeGenCourseProject.Codegen.Tests
{
    [TestClass()]
    public class CCodeGeneratorTests
    {
        private List<string> Run(string program, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = program;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = args;
            startInfo.RedirectStandardOutput = true;


            var output = new List<string>();
            try
            {

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.OutputDataReceived +=
                        (sender, arg) => output.Add(arg.Data);
                    process.Start();
                    process.BeginOutputReadLine();
                    process.WaitForExit();
                }

                return output;
  
            }
            catch
            {
                Assert.Fail();
            }
            return null;
        }
        private List<string> CompileAndRun(string file)
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Codegen\" + file, reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            node.Accept(semanticChecker);
            Assert.AreEqual(0, reporter.Errors.Count);

            var tacGenerator = new TACGenerator();
            node.Accept(tacGenerator);

            var cfgGenerator = new CFGGenerator(tacGenerator.Functions);

            var analyzer = new CFGAnalyzer(
                reporter,
                tacGenerator.Functions,
                cfgGenerator.GenerateCFG());
            analyzer.Analyze();
            Assert.AreEqual(0, reporter.Errors.Count);

            var generator = new CCodeGenerator(tacGenerator.Functions);
            generator.GenerateCode();

            using (var stream = new FileStream("out.c", FileMode.Create))
            {
                generator.SaveResult(stream);
            }

            Run("gcc", "out.c");
            return Run("a.exe", "");
        }
        [TestMethod()]
        public void IntegerFactorialProducesCorrectOutput()
        {
            var output = CompileAndRun("integer_factorial.txt");
            Assert.AreEqual("720", output[0]);
        }
    }
}