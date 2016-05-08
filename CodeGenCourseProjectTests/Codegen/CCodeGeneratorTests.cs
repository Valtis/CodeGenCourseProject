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
        public void IntegerExpressions()
        {
            var output = CompileAndRun("integer_expressions.txt");
            Assert.AreEqual(20, output.Count);
            Assert.AreEqual("4", output[0]);
            Assert.AreEqual("6", output[1]);
            Assert.AreEqual("-8", output[2]);
            Assert.AreEqual("36", output[3]);
            Assert.AreEqual("2", output[4]);
            Assert.AreEqual("6", output[5]);
            Assert.AreEqual("6", output[6]);
            Assert.AreEqual("29", output[7]);
            Assert.AreEqual("-28", output[8]);
            Assert.AreEqual("-84", output[9]);
            Assert.AreEqual("-14", output[10]);
            Assert.AreEqual("14", output[11]);
            Assert.AreEqual("0", output[12]);
            Assert.AreEqual("5", output[13]);
            Assert.AreEqual("23", output[14]);
            Assert.AreEqual("16", output[15]);
            Assert.AreEqual("11", output[16]);
            Assert.AreEqual("164", output[17]);
            Assert.AreEqual("123", output[18]);
            Assert.AreEqual(null, output[19]);
        }

        [TestMethod()]
        public void RealExpressions()
        {
            var output = CompileAndRun("real_expressions.txt");
            Assert.AreEqual(9, output.Count);
            Assert.AreEqual("0.000000", output[0]);
            Assert.AreEqual("2.100000", output[1]);
            Assert.AreEqual("-0.034000", output[2]);
            Assert.AreEqual("20.740734", output[3]);
            Assert.AreEqual("50.000000", output[4]);
            Assert.AreEqual("-0.000023", output[5]);
            Assert.AreEqual("-0.000023", output[6]);
            Assert.AreEqual("123.456000", output[7]);
            Assert.AreEqual(null, output[8]);
        }

        [TestMethod()]
        public void BooleanExpressions()
        {
            var output = CompileAndRun("boolean_expressions.txt");
            Assert.AreEqual(33, output.Count);

            Assert.AreEqual("1", output[0]);
            Assert.AreEqual("0", output[1]);
            Assert.AreEqual("0", output[2]);

            Assert.AreEqual("1", output[3]);
            Assert.AreEqual("1", output[4]);
            Assert.AreEqual("0", output[5]);

            Assert.AreEqual("0", output[6]);
            Assert.AreEqual("1", output[7]);
            Assert.AreEqual("0", output[8]);

            Assert.AreEqual("0", output[9]);
            Assert.AreEqual("1", output[10]);
            Assert.AreEqual("1", output[11]);

            Assert.AreEqual("0", output[12]);
            Assert.AreEqual("0", output[13]);
            Assert.AreEqual("1", output[14]);

            Assert.AreEqual("1", output[15]);
            Assert.AreEqual("0", output[16]);
            Assert.AreEqual("1", output[17]);

            Assert.AreEqual("1", output[18]);
            Assert.AreEqual("0", output[19]);

            Assert.AreEqual("1", output[20]);
            Assert.AreEqual("0", output[21]);
            Assert.AreEqual("0", output[22]);
            Assert.AreEqual("0", output[23]);

            Assert.AreEqual("1", output[24]);
            Assert.AreEqual("1", output[25]);
            Assert.AreEqual("1", output[26]);
            Assert.AreEqual("0", output[27]);

            Assert.AreEqual("0", output[28]);
            Assert.AreEqual("1", output[29]);

            Assert.AreEqual("0", output[30]);
            Assert.AreEqual("1", output[31]);


            Assert.AreEqual(null, output[32]);
        }
        
        [TestMethod()]
        public void StringExpressions()
        {
            var output = CompileAndRun("string_expressions.txt");
            Assert.AreEqual(24, output.Count);
            Assert.AreEqual("test", output[0]);
            Assert.AreEqual("test2", output[1]);
            Assert.AreEqual("this is a test2", output[2]);
            Assert.AreEqual("write  directly", output[3]);

            Assert.AreEqual("1", output[4]);
            Assert.AreEqual("0", output[5]);
            Assert.AreEqual("0", output[6]);

            Assert.AreEqual("1", output[7]);
            Assert.AreEqual("1", output[8]);
            Assert.AreEqual("0", output[9]);

            Assert.AreEqual("0", output[10]);
            Assert.AreEqual("1", output[11]);
            Assert.AreEqual("0", output[12]);

            Assert.AreEqual("0", output[13]);
            Assert.AreEqual("1", output[14]);
            Assert.AreEqual("1", output[15]);

            Assert.AreEqual("0", output[16]);
            Assert.AreEqual("0", output[17]);
            Assert.AreEqual("1", output[18]);

            Assert.AreEqual("1", output[19]);
            Assert.AreEqual("0", output[20]);
            Assert.AreEqual("1", output[21]);

            Assert.AreEqual("abcdefghijkl", output[22]);

            Assert.AreEqual(null, output[23]);
        }

        [TestMethod()]
        public void ArrayLowerBound()
        {
            var output = CompileAndRun("array_lower_bound_check.txt");
            Assert.AreEqual("Array index out of bounds at line 4: Was -1 when array has size 3", output[0]);
        }

        [TestMethod()]
        public void ArrayUpperBound()
        {
            var output = CompileAndRun("array_upper_bound_check.txt");
            Assert.AreEqual("Array index out of bounds at line 4: Was 3 when array has size 3", output[0]);
        }

        [TestMethod()]
        public void Writeln()
        {
            var output = CompileAndRun("writeln.txt");
            Assert.AreEqual(7, output.Count);
            Assert.AreEqual("2", output[0]);
            Assert.AreEqual("4.200000", output[1]);
            Assert.AreEqual("1", output[2]);
            Assert.AreEqual("0", output[3]);
            Assert.AreEqual("hello", output[4]);
            Assert.AreEqual("A: 4\tB: 9", output[5]);
            Assert.AreEqual(null, output[6]);
        }

        [TestMethod()]
        public void NonReferenceNonArrayParameters()
        {
            var output = CompileAndRun("non_reference_non_array_parameters.txt");
            Assert.AreEqual(13, output.Count);
            Assert.AreEqual("12", output[0]);
            Assert.AreEqual("12.350000", output[1]);
            Assert.AreEqual("inner", output[2]);
            Assert.AreEqual("1", output[3]);
            Assert.AreEqual("0", output[4]);
            Assert.AreEqual("0.000000", output[5]);
            Assert.AreEqual("non_assign", output[6]);
            Assert.AreEqual("0", output[7]);
            Assert.AreEqual("12", output[8]);
            Assert.AreEqual("12.350000", output[9]);
            Assert.AreEqual("inner", output[10]);
            Assert.AreEqual("1", output[11]);
            Assert.AreEqual(null, output[12]);
        }
        
        [TestMethod()]
        public void ReferenceNonArrayParameters()
        {
            var output = CompileAndRun("reference_non_array_parameters.txt");
            Assert.AreEqual(5, output.Count);
            Assert.AreEqual("24", output[0]);
            Assert.AreEqual("987.654000", output[1]);
            Assert.AreEqual("inner", output[2]);
            Assert.AreEqual("1", output[3]);
            Assert.AreEqual(null, output[4]);
        }

        [TestMethod]
        public void NonReferenceArrayParameters()
        {
            var output = CompileAndRun("non_reference_array_parameters.txt");
            Assert.AreEqual(33, output.Count);
            Assert.AreEqual("123", output[0]);
            Assert.AreEqual("456", output[1]);
            Assert.AreEqual("789", output[2]);
            Assert.AreEqual("101112", output[3]);
            Assert.AreEqual("123.000000", output[4]);
            Assert.AreEqual("456.000000", output[5]);
            Assert.AreEqual("-789.000000", output[6]);
            Assert.AreEqual("-101112.000000", output[7]);
            Assert.AreEqual("aa", output[8]);
            Assert.AreEqual("bb", output[9]);
            Assert.AreEqual("cc", output[10]);
            Assert.AreEqual("dd", output[11]);
            Assert.AreEqual("1", output[12]);
            Assert.AreEqual("0", output[13]);
            Assert.AreEqual("0", output[14]);
            Assert.AreEqual("1", output[15]);
            Assert.AreEqual("123", output[16]);
            Assert.AreEqual("456", output[17]);
            Assert.AreEqual("789", output[18]);
            Assert.AreEqual("101112", output[19]);
            Assert.AreEqual("123.000000", output[20]);
            Assert.AreEqual("456.000000", output[21]);
            Assert.AreEqual("-789.000000", output[22]);
            Assert.AreEqual("-101112.000000", output[23]);
            Assert.AreEqual("aa", output[24]);
            Assert.AreEqual("bb", output[25]);
            Assert.AreEqual("cc", output[26]);
            Assert.AreEqual("dd", output[27]);
            Assert.AreEqual("1", output[28]);
            Assert.AreEqual("0", output[29]);
            Assert.AreEqual("0", output[30]);
            Assert.AreEqual("1", output[31]);
            Assert.AreEqual(null, output[32]);
        }
        [TestMethod]
        public void ReferenceArrayParameters()
        {
            var output = CompileAndRun("reference_array_parameters.txt");
            Assert.AreEqual(33, output.Count);
            Assert.AreEqual("123", output[0]);
            Assert.AreEqual("456", output[1]);
            Assert.AreEqual("789", output[2]);
            Assert.AreEqual("101112", output[3]);
            Assert.AreEqual("123.000000", output[4]);
            Assert.AreEqual("456.000000", output[5]);
            Assert.AreEqual("-789.000000", output[6]);
            Assert.AreEqual("-101112.000000", output[7]);
            Assert.AreEqual("aa", output[8]);
            Assert.AreEqual("bb", output[9]);
            Assert.AreEqual("cc", output[10]);
            Assert.AreEqual("dd", output[11]);
            Assert.AreEqual("1", output[12]);
            Assert.AreEqual("0", output[13]);
            Assert.AreEqual("0", output[14]);
            Assert.AreEqual("1", output[15]);
            Assert.AreEqual("0", output[16]);
            Assert.AreEqual("0", output[17]);
            Assert.AreEqual("0", output[18]);
            Assert.AreEqual("0", output[19]);
            Assert.AreEqual("0.000000", output[20]);
            Assert.AreEqual("0.000000", output[21]);
            Assert.AreEqual("0.000000", output[22]);
            Assert.AreEqual("0.000000", output[23]);
            Assert.AreEqual("", output[24]);
            Assert.AreEqual("", output[25]);
            Assert.AreEqual("", output[26]);
            Assert.AreEqual("", output[27]);
            Assert.AreEqual("0", output[28]);
            Assert.AreEqual("0", output[29]);
            Assert.AreEqual("0", output[30]);
            Assert.AreEqual("0", output[31]);
            Assert.AreEqual(null, output[32]);
        }


        [TestMethod()]
        public void ArrayAssignment()
        {
            Assert.Fail();
        }


        [TestMethod()]
        public void ArrayInitializationWithNegativeNumber()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void AssignValueFromReference()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ArraySizeOperatorWithReference()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IndexedArrayAsArgumentToReferenceOrNonreferenceAndReferenceArrayAsArgumentEtc()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void WritelnandReadArgsWhenRefs()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void IterativeFactorial()
        {
            var output = CompileAndRun("iterative_factorial.txt");
            Assert.AreEqual("720", output[0]);
        }

        [TestMethod()]
        public void RecursiveFactorial()
        {
            var output = CompileAndRun("recursive_factorial.txt");
            Assert.AreEqual("3628800", output[0]);
        }

        [TestMethod()]
        public void IterativeFibonacci()
        {
            var output = CompileAndRun("iterative_fibonacci.txt");
            Assert.AreEqual("144", output[0]);
        }

        [TestMethod()]
        public void RecursiveFibonacci()
        {
            var output = CompileAndRun("recursive_fibonacci.txt");
            Assert.AreEqual("6765", output[0]);
        }

        [TestMethod()]
        public void HelloWorld()
        {
            var output = CompileAndRun("hello_world.txt");
            Assert.AreEqual("Hello, world!", output[0]);
        }
    }
}