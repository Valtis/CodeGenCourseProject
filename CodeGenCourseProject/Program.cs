using CodeGenCourseProject.CFG;
using CodeGenCourseProject.CFG.Analysis;
using CodeGenCourseProject.Codegen;
using CodeGenCourseProject.Codegen.C;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.Parsing;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC;
using System;
using System.IO;

namespace CodeGenCourseProject
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DoCompile(args);
            }
            catch (InternalCompilerError e)
            {
                Console.WriteLine("Internal compiler error: " + e.ToString());
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException || e is FileNotFoundException)
                {
                    Console.WriteLine("Could not find the file '" + args[0] + "'");
                }
                else
                {
                    throw;
                }
            }
        }

        private static void DoCompile(string [] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Mini-pascal file must be provided as command line argument");
                return;
            }

            var reporter = new MessageReporter();
            var lexer = new Lexer(args[0], reporter);

            var parser = new Parser(lexer, reporter);
            var root = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            root.Accept(semanticChecker);

            if (reporter.Errors.Count != 0)
            {
                reporter.PrintMessages();
                Console.WriteLine("\n\nAborting compilation");
                return;
            }

            var tacGenerator = new Generator();

            root.Accept(tacGenerator);

            var cfgGenerator = new CFGGenerator(tacGenerator.Functions);
            var graph = cfgGenerator.GenerateCFG();

            var cfgAnalyzer = new CFGAnalyzer(reporter, tacGenerator.Functions, graph);
            cfgAnalyzer.Analyze();

            reporter.PrintMessages();
            if (reporter.Errors.Count != 0)
            {
                Console.WriteLine("\n\nAborting compilation");
                return;
            }

            var generator = new CCodeGenerator(tacGenerator.Functions);
            generator.GenerateCode();

            using (var stream = new FileStream(tacGenerator.ProgramName + ".c", FileMode.Create))
            {
                generator.SaveResult(stream);
            }
        }
    }
}
