using CodeGenCourseProject.CFG;
using CodeGenCourseProject.CFG.Analysis;
using CodeGenCourseProject.Codegen;
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
            var reporter = new ErrorReporter();
            var common = @"C:\Users\Erkka\documents\visual studio 2015\Projects\CodeGenCourseProject\CodeGenCourseProjectTests\";
            var parser_prefix = common + @"Parsing\";
            var semantic_prefix = common + @"SemanticChecking\";
            var tac_prefix = common + @"TAC\";
            var cfgPrefix = common + @"CFG\";
            var cfgAnalysisPrefix = common + @"CFG\Analysis\";

            var lexer = new Lexer(
              //semantic_prefix + "invalid_ref_args.txt",
             //    tac_prefix + "asserts.txt",
            //     cfgPrefix + "functions.txt",
                cfgAnalysisPrefix + "invalid_jumps.txt",
                reporter);

            var parser = new Parser(lexer, reporter);
            var root = parser.Parse();

            var semanticChecker = new SemanticChecker(reporter);
            root.Accept(semanticChecker);


            if (reporter.Errors.Count != 0)
            {
                reporter.PrintMessages();
                Console.WriteLine("\n\nAborting compilation");
                Console.ReadKey();
                return;
            }

            var tacGenerator = new TACGenerator();

            root.Accept(tacGenerator);

            foreach (var function in tacGenerator.Functions)
            {
                Console.WriteLine(function);
                foreach (var statements in function.Statements)
                {
                    Console.WriteLine("\t" + statements.ToString());
                }
            }

            var cfgGenerator = new CFGGenerator(tacGenerator.Functions);
            var graph = cfgGenerator.GenerateCFG();

            var cfgAnalyzer = new CFGAnalyzer(reporter, tacGenerator.Functions, graph);
            cfgAnalyzer.Analyze();

            reporter.PrintMessages();
            if (reporter.Errors.Count != 0)
            {
                Console.WriteLine("\n\nAborting compilation");
                Console.ReadKey();
                return;
            }
            /*
            var generator = new CCodeGenerator(tacGenerator.Functions);
            generator.GenerateCode();

            using (var stream = new FileStream("foo.c", FileMode.Create))
            {
                generator.SaveResult(stream);
            }
            
    */
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
