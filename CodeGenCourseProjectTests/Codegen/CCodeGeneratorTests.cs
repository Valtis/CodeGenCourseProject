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
using CodeGenCourseProject.Codegen.C;
using System;
using System.Threading;

namespace CodeGenCourseProject.Codegen.Tests
{
    [TestClass()]
    public class CCodeGeneratorTests
    {
        private List<string> Run(string program, string args, IList<string> stdin)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = program;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = args;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;


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

                    if (stdin != null)
                    {
                        foreach (var line in stdin)
                        {
                            process.StandardInput.WriteLine(line);
                        }
                    }


                    process.WaitForExit();
                }

                return output;

            }
            catch (Exception e)
            {
                Assert.Fail(program + ": " + e.ToString());
            }
            return null;
        }
        private List<string> CompileAndRun(string file, string extraArgs = "", IList<string> stdin = null)
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

            var name = "test_" + file;

            using (var stream = new FileStream(name + ".c", FileMode.Create))
            {
                generator.SaveResult(stream);
            }


            Run("gcc", name + ".c -o " + name + ".exe" + " " + extraArgs, stdin);
            var output = Run(name + ".exe", "", stdin);
            return output;

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
            Assert.AreEqual(9, output.Count);
            Assert.AreEqual("2", output[0]);
            Assert.AreEqual("4.200000", output[1]);
            Assert.AreEqual("1", output[2]);
            Assert.AreEqual("0", output[3]);
            Assert.AreEqual("hello", output[4]);
            Assert.AreEqual("A: 4\tB: 9", output[5]);
            Assert.AreEqual("8 4.100000 foo 1", output[6]);
            Assert.AreEqual("160 312.100000 rarar 0", output[7]);
            Assert.AreEqual(null, output[8]);
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
            Assert.AreEqual(34, output.Count);
            Assert.AreEqual("123 123.000000 aa 1", output[0]);
            Assert.AreEqual("123", output[1]);
            Assert.AreEqual("456", output[2]);
            Assert.AreEqual("789", output[3]);
            Assert.AreEqual("101112", output[4]);
            Assert.AreEqual("123.000000", output[5]);
            Assert.AreEqual("456.000000", output[6]);
            Assert.AreEqual("-789.000000", output[7]);
            Assert.AreEqual("-101112.000000", output[8]);
            Assert.AreEqual("aa", output[9]);
            Assert.AreEqual("bb", output[10]);
            Assert.AreEqual("cc", output[11]);
            Assert.AreEqual("dd", output[12]);
            Assert.AreEqual("1", output[13]);
            Assert.AreEqual("0", output[14]);
            Assert.AreEqual("0", output[15]);
            Assert.AreEqual("1", output[16]);
            Assert.AreEqual("123", output[17]);
            Assert.AreEqual("456", output[18]);
            Assert.AreEqual("789", output[19]);
            Assert.AreEqual("101112", output[20]);
            Assert.AreEqual("123.000000", output[21]);
            Assert.AreEqual("456.000000", output[22]);
            Assert.AreEqual("-789.000000", output[23]);
            Assert.AreEqual("-101112.000000", output[24]);
            Assert.AreEqual("aa", output[25]);
            Assert.AreEqual("bb", output[26]);
            Assert.AreEqual("cc", output[27]);
            Assert.AreEqual("dd", output[28]);
            Assert.AreEqual("1", output[29]);
            Assert.AreEqual("0", output[30]);
            Assert.AreEqual("0", output[31]);
            Assert.AreEqual("1", output[32]);
            Assert.AreEqual(null, output[33]);
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
        public void IndexedArrayValuesAsArgs()
        {
            var output = CompileAndRun("array_indexes_as_args.txt");
            Assert.AreEqual(5, output.Count);
            Assert.AreEqual("0", output[0]);
            Assert.AreEqual("2", output[1]);
            Assert.AreEqual("2", output[2]);
            Assert.AreEqual("26", output[3]);
            Assert.AreEqual(null, output[4]);
        }

        [TestMethod()]
        public void ArrayAssignment()
        {
            var output = CompileAndRun("array_assign.txt");
            Assert.AreEqual(13, output.Count);
            Assert.AreEqual("0 0.000000  0", output[0]);
            Assert.AreEqual("0 1.100000 foo 1", output[1]);
            Assert.AreEqual("0 0.000000  0", output[2]);
            Assert.AreEqual("0 0.000000  0", output[3]);
            Assert.AreEqual("1 1.100000 foo 1", output[4]);
            Assert.AreEqual("0 0.000000  0", output[5]);
            Assert.AreEqual("0 0.000000  0", output[6]);
            Assert.AreEqual("2 1.100000 foo 1", output[7]);
            Assert.AreEqual("0 0.000000  0", output[8]);
            Assert.AreEqual("0 0.000000  0", output[9]);
            Assert.AreEqual("3 1.100000 foo 1", output[10]);
            Assert.AreEqual("0 0.000000  0", output[11]);
            Assert.AreEqual(null, output[12]);
        }


        [TestMethod()]
        public void ArrayInitializationWithNegativeNumber()
        {
            var output = CompileAndRun("negative_array_size.txt");
            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("Invalid array size at line 5: -2", output[0]);
            Assert.AreEqual(null, output[1]);
        }

        [TestMethod()]
        public void AssignValueFromReference()
        {
            var output = CompileAndRun("assign_reference_value.txt");
            Assert.AreEqual(17, output.Count);
            Assert.AreEqual("1", output[0]);
            Assert.AreEqual("23", output[1]);
            Assert.AreEqual("1.000000", output[2]);
            Assert.AreEqual("2.000000", output[3]);
            Assert.AreEqual("a", output[4]);
            Assert.AreEqual("prefix_a", output[5]);
            Assert.AreEqual("1", output[6]);
            Assert.AreEqual("1", output[7]);

            Assert.AreEqual("12", output[8]);
            Assert.AreEqual("276", output[9]);
            Assert.AreEqual("1.400000", output[10]);
            Assert.AreEqual("2.800000", output[11]);
            Assert.AreEqual("foo", output[12]);
            Assert.AreEqual("prefix_foo", output[13]);
            Assert.AreEqual("0", output[14]);
            Assert.AreEqual("1", output[15]);
            Assert.AreEqual(null, output[16]);
        }

        [TestMethod()]
        public void ArraySize()
        {
            var output = CompileAndRun("array_size.txt");
            Assert.AreEqual(11, output.Count);
            Assert.AreEqual("10", output[0]);
            Assert.AreEqual("20", output[1]);
            Assert.AreEqual("10", output[2]);
            Assert.AreEqual("0", output[3]);
            Assert.AreEqual("0", output[4]);
            Assert.AreEqual("10", output[5]);
            Assert.AreEqual("10", output[6]);
            Assert.AreEqual("20", output[7]);
            Assert.AreEqual("20", output[8]);
            Assert.AreEqual("20", output[9]);
            Assert.AreEqual(null, output[10]);
        }

        [TestMethod()]
        public void VariableCapture()
        {
            var output = CompileAndRun("variable_capture.txt");
            Assert.AreEqual(10, output.Count);
            Assert.AreEqual("hello 12 2421 4", output[0]);
            Assert.AreEqual("12 2.300000 19 1234567", output[1]);
            Assert.AreEqual("12", output[2]);
            Assert.AreEqual("12", output[3]);
            Assert.AreEqual("22", output[4]);
            Assert.AreEqual("76", output[5]);
            Assert.AreEqual("76", output[6]);
            Assert.AreEqual("1", output[7]);
            Assert.AreEqual("14", output[8]);
            Assert.AreEqual(null, output[9]);
        }

        [TestMethod()]
        public void FalseAssert()
        {
            var output = CompileAndRun("false_assert.txt");
            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("Assert failed at line 5", output[0]);
            Assert.AreEqual(null, output[1]);
        }

        [TestMethod()]
        public void TrueAssert()
        {
            var output = CompileAndRun("true_assert.txt");
            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("This should be printed", output[0]);
            Assert.AreEqual(null, output[1]);
        }

        [TestMethod()]
        public void GC1()
        {
            var output = CompileAndRun("gc.txt", "-DGC_DEBUG -DMAX_HEAP_SIZE=100");
            Assert.AreEqual(37, output.Count);
            // Skipping uninteresting lines
            Assert.AreEqual("Initializing GC", output[0]);
            Assert.AreEqual("MAX_HEAP_SIZE: 100", output[1]);
            Assert.AreEqual("Collecting dead objects", output[5]);
            Assert.AreEqual("Memory in use: 80 bytes", output[6]);
            Assert.AreEqual("GC finished", output[11]);
            Assert.AreEqual("Memory in use: 60 bytes", output[12]);
            Assert.AreEqual("Collecting dead objects", output[16]);
            Assert.AreEqual("Memory in use: 100 bytes", output[17]);
            Assert.AreEqual("GC finished", output[23]);
            Assert.AreEqual("Memory in use: 60 bytes", output[24]);
            Assert.AreEqual("Collecting dead objects", output[26]);
            Assert.AreEqual("Memory in use: 80 bytes", output[27]);
            Assert.AreEqual("GC finished", output[34]);
            Assert.AreEqual("Memory in use: 80 bytes", output[35]);
            Assert.AreEqual(null, output[36]);
        }

        [TestMethod()]
        public void GC2()
        {
            var output = CompileAndRun("gc2.txt", "-DGC_DEBUG -DMAX_HEAP_SIZE=100");
            Assert.AreEqual(37, output.Count);
            // Skipping uninteresting lines
            Assert.AreEqual("Initializing GC", output[0]);
            Assert.AreEqual("MAX_HEAP_SIZE: 100", output[1]);
            Assert.AreEqual("Collecting dead objects", output[5]);
            Assert.AreEqual("Memory in use: 64 bytes", output[6]);
            Assert.AreEqual("String array - scanning", output[11]);
            Assert.AreEqual("GC finished", output[14]);
            Assert.AreEqual("Memory in use: 24 bytes", output[15]);
            Assert.AreEqual("Collecting dead objects", output[19]);
            Assert.AreEqual("Memory in use: 94 bytes", output[20]);
            Assert.AreEqual("String array - scanning", output[25]);
            Assert.AreEqual("GC finished", output[30]);
            Assert.AreEqual("Memory in use: 54 bytes", output[31]);
            Assert.AreEqual("hello---------", output[33]);
            Assert.AreEqual("world---------", output[34]);
            Assert.AreEqual("test----------", output[35]);
            Assert.AreEqual(null, output[36]);
        }

        [TestMethod()]
        public void Read()
        {
            var output = CompileAndRun("read.txt", "",
                new List<string> {
                    "123 34e2 hello",
                    "567 32.12 capture()",
                    "1243 15e-1 non-ref-arg",
                    "987654, 2.5e2 ref-arg",
                    "1 1.1 array-index",
                    "2 2.2 array-args",
                    "3 3.3 array-ref-args",
                    "4 4.4 array-param",
                    "5 5.5 array-ref-param",
                    "6 6.6 array-capture",
                    ""
                });
            Assert.AreEqual(20, output.Count);
            Assert.AreEqual("123 3400.000000 hello", output[0]);
            Assert.AreEqual("567 32.120000 capture()", output[1]);
            Assert.AreEqual("567 32.120000 capture()", output[2]);
            Assert.AreEqual("1243 1.500000 non-ref-arg", output[3]);
            Assert.AreEqual("567 32.120000 capture()", output[4]);
            Assert.AreEqual("987654 250.000000 ref-arg", output[5]);
            Assert.AreEqual("987654 250.000000 ref-arg", output[6]);
            Assert.AreEqual("1 1.100000 array-index", output[7]);
            Assert.AreEqual("2 2.200000 array-args", output[8]);
            Assert.AreEqual("1 1.100000 array-index", output[9]);
            Assert.AreEqual("3 3.300000 array-ref-args", output[10]);
            Assert.AreEqual("3 3.300000 array-ref-args", output[11]);
            Assert.AreEqual("4 4.400000 array-param", output[12]);
            Assert.AreEqual("3 3.300000 array-ref-args", output[13]);
            Assert.AreEqual("5 5.500000 array-ref-param", output[14]);
            Assert.AreEqual("5 5.500000 array-ref-param", output[15]);
            Assert.AreEqual("6 6.600000 array-capture", output[16]);
            Assert.AreEqual("6 6.600000 array-capture", output[17]);
            Assert.AreEqual("0 0.000000 ", output[18]);
            Assert.AreEqual(null, output[19]);
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

        [TestMethod()]
        public void EscapeSequences()
        {
            var output = CompileAndRun("escape_sequences.txt");
            Assert.AreEqual(10, output.Count);
            Assert.AreEqual("Hello\tWorld", output[0]);
            Assert.AreEqual("Hello", output[1]);
            Assert.AreEqual("World", output[2]);
            Assert.AreEqual("Hello", output[3]);
            Assert.AreEqual("World", output[4]);
            Assert.AreEqual("Hello\"World\"", output[5]);
            Assert.AreEqual("Hello\\World", output[6]);
            // 0X1B is \e, but as it is non-standard, C# rejects the escape-sequence
            Assert.AreEqual("Hello" + (char)0x1B +  "World", output[7]);
            Assert.AreEqual("Hello\bWorld", output[8]);
            Assert.AreEqual(null, output[9]);
        }

        [TestMethod()]
        public void EvaluationOrder()
        {
            var output = CompileAndRun("left_to_right_evaluation.txt");
            Assert.AreEqual(7, output.Count);
            Assert.AreEqual("First", output[0]);
            Assert.AreEqual("second", output[1]);

            Assert.AreEqual("First", output[2]);
            Assert.AreEqual("second", output[3]);
            Assert.AreEqual("second", output[4]);
            Assert.AreEqual("third", output[5]);
            Assert.AreEqual(null, output[6]);
        }

        [TestMethod()]
        public void Expr2()
        {
            var output = CompileAndRun("expr2.txt");
            Assert.AreEqual(7, output.Count);
            Assert.AreEqual("26", output[0]);
            Assert.AreEqual("hello world!!!", output[1]);
            Assert.AreEqual("-1.100000", output[2]);
            Assert.AreEqual("-3", output[3]);
            Assert.AreEqual("0", output[4]);
            Assert.AreEqual("1", output[5]);
            Assert.AreEqual(null, output[6]);
        }

        /*
        Test case for a bug that was found during manual testing.
        Captured array was treated as non-reference and it generated broken C code
        */
        [TestMethod()]
        public void PassCapturedArrayAsArgument()
        {
            var output = CompileAndRun("captured_array_as_argument.txt");
            Assert.AreEqual(6, output.Count);
            Assert.AreEqual("1 2 3", output[0]);
            Assert.AreEqual("4 5 6", output[1]);
            Assert.AreEqual("1 2 3", output[2]);
            Assert.AreEqual("4 5 6", output[3]);
            Assert.AreEqual("4 5 6", output[4]);
            Assert.AreEqual(null, output[5]);
        }

        /*
        Test case for a bug that was found during manual testing.
        Captured array indexes and reference arg as index were not dereferenced correctly
        */
        [TestMethod()]
        public void CapturedArrayIndex()
        {
            var output = CompileAndRun("array_index_captured_ref.txt");
            Assert.AreEqual(3, output.Count);
            Assert.AreEqual("6", output[0]);
            Assert.AreEqual("7", output[1]);
            Assert.AreEqual(null, output[2]);
        }
        /*
          Test case for a bug that was found during manual testing.
          Array returning was completely broken - turns out no test case was testing this!
        */
        [TestMethod()]
        public void ArrayReturnBug()
        {
            var output = CompileAndRun("array_return_bug.txt");
            Assert.AreEqual(3, output.Count);
            Assert.AreEqual("2", output[0]);
            Assert.AreEqual("6 8", output[1]);
            Assert.AreEqual(null, output[2]);
        }

        /*
          Test case for a bug that was found during manual testing.
          References were not dereferenced correctly when returning them
        */
        [TestMethod()]
        public void ReferenceReturnBug()
        {
            var output = CompileAndRun("return_reference.txt");
            Assert.AreEqual(5, output.Count);
            Assert.AreEqual("2", output[0]);
            Assert.AreEqual("6 8", output[1]);
            Assert.AreEqual("23.400000", output[2]);
            Assert.AreEqual("0", output[3]);
            Assert.AreEqual(null, output[4]);
        }
    }
}