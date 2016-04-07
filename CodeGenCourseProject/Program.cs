using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using System;

namespace CodeGenCourseProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(
                @"C:\Users\Erkka\documents\visual studio 2015\Projects\CodeGenCourseProject\CodeGenCourseProjectTests\Lexing\invalid_comments.txt",
                reporter);

            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());
            Console.WriteLine(lexer.NextToken());

            Console.WriteLine(lexer.NextToken());



            reporter.PrintMessages();


            Console.ReadKey();
        }
    }
}
