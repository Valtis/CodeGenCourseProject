using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.AST;
using System.Collections.Generic;
using CodeGenCourseProject.Tokens;

namespace CodeGenCourseProject.Parsing.Tests
{
    [TestClass()]
    public class ParserTests
    {
        [TestMethod()]
        public void MinimalProgramIsAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_minimal_program.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(
                    0, 0, new IdentifierToken("foo"),
                    new BlockNode(0, 0,
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "a"),
                            new IntegerNode(0, 0, 4)))
                ),
                node);
        }

        [TestMethod()]
        public void MinimalProgramWithoutTrailingSemicolonIsAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_minimal_program_without_trailing_semicolon.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(
                    0, 0, new IdentifierToken("test"),
                    new BlockNode(0, 0,
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "a"),
                            new IntegerNode(0, 0, 4)),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "b"),
                            new IntegerNode(0, 0, 5)),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "c"),
                            new IntegerNode(0, 0, 6))
                    )
                ),
                node);
        }

        [TestMethod()]
        public void MinimalProgramWithTrailingSemicolonIsAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_minimal_program_with_trailing_semicolon.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(
                    0, 0, new IdentifierToken("test"),
                    new BlockNode(0, 0,
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "a"),
                            new IntegerNode(0, 0, 4)),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "b"),
                            new IntegerNode(0, 0, 1)),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "c"),
                            new IntegerNode(0, 0, 24))
                    )
                ),
                node);
        }

        [TestMethod()]
        public void ProgramWithMissingSemicolonsInMiddleOfStatementBlockIsRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_program_with_missing_semicolon_in_mid_statement.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("missing_semicolon"),
                    new BlockNode(0, 0,
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "a"),
                            new IntegerNode(0, 0, 232)
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "b"),
                            new IntegerNode(0, 0, 1215)
                        ),
                        new ErrorNode(),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "d"),
                            new RealNode(0, 0, 4.124e2)
                        )
                    )
                ),
               node
            );

            Assert.AreEqual(1, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ProgramWithMissingDotIsRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_program_with_missing_dot.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();
            ASTMatches(
               new ProgramNode(
                   0, 0, new IdentifierToken("no_dot"),
                   new BlockNode(0, 0,
                       new VariableAssignmentNode(0, 0,
                           new IdentifierNode(0, 0, "a"),
                           new IntegerNode(0, 0, 4))
                   )
               ),
               node
            );

            Assert.AreEqual(1, reporter.Errors.Count);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(2, reporter.Errors[0].Line);
            Assert.AreEqual(3, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Expected token <operator - '.'>"));
        }

        [TestMethod()]
        public void ProgramWithMissingProgramKeywordIsRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_minimal_program_1.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(1, reporter.Errors.Count);

            ASTMatches(
                new ErrorNode(),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(0, reporter.Errors[0].Line);
            Assert.AreEqual(0, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Expected token <keyword - 'program'> but was actually <identifier - 'foo'>"));
        }

        [TestMethod()]
        public void ProgramWithMissingInitialBeginKeywordIsRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_minimal_program_2.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(1, reporter.Errors.Count);

            ASTMatches(
                new ErrorNode(),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(1, reporter.Errors[0].Line);
            Assert.AreEqual(8, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Expected token <keyword - 'begin'> but was actually <identifier - 'a'>"));
        }

        [TestMethod()]
        public void ValidExpressionsAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_expressions.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("expression_test"),
                    new BlockNode(0, 0,
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "assignment"),
                            new IntegerNode(0, 0, 4)
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "assinment_with_real"),
                            new RealNode(0, 0, 1432.21)
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "assignment_with_variable"),
                            new IdentifierNode(0, 0, "hello")
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_with_string"),
                            new StringNode(0, 0, "hello")
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_with_negation"),
                            new IntegerNode(0, 0, -233)
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_with_real_negation"),
                            new RealNode(0, 0, -235.12)
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_with_variable_negation"),
                            new NegateNode(0, 0,
                                new IdentifierNode(0, 0, "hello")
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_with_not_and_identifier"),
                            new NotNode(0, 0,
                                new IdentifierNode(0, 0, "hello")
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_with_not_and_integer"),
                            new NotNode(0, 0,
                                new IntegerNode(0, 0, 346)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_with_parenthesis"),
                            new IntegerNode(0, 0, 235)
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_of_array_value"),
                            new ArrayIndexNode(0, 0, new IdentifierToken("arr"),
                                new IntegerNode(0, 0, 4)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_with_array_index_negation"),
                            new ArrayIndexNode(0, 0, new IdentifierToken("arr"),
                                new IntegerNode(0, 0, -5)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_of_array_size"),
                            new ArraySizeNode(0, 0,
                                new IdentifierNode(0, 0, "foo")
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_of_array_size_with_parenthesis"),
                            new ArraySizeNode(0, 0,
                                new IdentifierNode(0, 0, "foo")
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_of_semantically_invalid_size"),
                            new ArraySizeNode(0, 0,
                                new IntegerNode(0, 0, -235)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "array_index_with_array_length"),
                            new ArrayIndexNode(0, 0, new IdentifierToken("arr"),
                                new ArraySizeNode(0, 0,
                                    new IdentifierNode(0, 0, "arr")
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "assignment_with_plus_sign"),
                            new IntegerNode(0, 0, 2)
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "array_access_with_plus_sign"),
                            new ArrayIndexNode(0, 0, new IdentifierToken("arr"),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "multiplication"),
                            new MultiplyNode(0, 0,
                                new IntegerNode(0, 0, 20),
                                new IntegerNode(0, 0, 40)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "division"),
                            new DivideNode(0, 0,
                                new IntegerNode(0, 0, 50),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "modulo"),
                            new ModuloNode(0, 0,
                                new IntegerNode(0, 0, 53),
                                new IntegerNode(0, 0, 34)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "boolean_expression_with_and"),
                            new AndNode(0, 0,
                                new IdentifierNode(0, 0, "hello"),
                                new IdentifierNode(0, 0, "world")
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "multiplication_with_array"),
                            new MultiplyNode(0, 0,
                                new ArrayIndexNode(0, 0, new IdentifierToken("a"),
                                    new IntegerNode(0, 0, 4)
                                ),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "multiplication_with_array2"),
                            new MultiplyNode(0, 0,
                                new IntegerNode(0, 0, 2),
                                new ArrayIndexNode(0, 0, new IdentifierToken("b"),
                                    new IntegerNode(0, 0, 13)
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "multiplication_with_not"),
                            new MultiplyNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new NotNode(0, 0,
                                    new IdentifierNode(0, 0, "b")
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "division_with_not"),
                            new DivideNode(0, 0,
                                new NotNode(0, 0,
                                    new IdentifierNode(0, 0, "a")
                                ),
                                new IdentifierNode(0, 0, "b")
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "modulo_with_array_size"),
                            new ModuloNode(0, 0,
                                new ArraySizeNode(0, 0,
                                    new IdentifierNode(0, 0, "foo")
                                ),
                                new RealNode(0, 0, 25.32)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "multiplication_with_negation"),
                            new NegateNode(0, 0,
                                new MultiplyNode(0, 0,
                                    new IntegerNode(0, 0, 4),
                                    new IntegerNode(0, 0, 25)
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "multiplication_with_negation_and_parenthesis"),
                            new MultiplyNode(0, 0,
                                new IntegerNode(0, 0, -4),
                                new IntegerNode(0, 0, 25)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "division_with_plus_sign"),
                            new DivideNode(0, 0,
                                new RealNode(0, 0, 594.24e2),
                                new IntegerNode(0, 0, 25)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "addition"),
                            new AddNode(0, 0,
                                new IntegerNode(0, 0, 4),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "subtraction"),
                            new SubtractNode(0, 0,
                                new IntegerNode(0, 0, 5),
                                new IntegerNode(0, 0, 6)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "boolean_expression_with_or"),
                            new OrNode(0, 0,
                                new IdentifierNode(0, 0, "hello"),
                                new IdentifierNode(0, 0, "world")
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "addition_with_negation"),
                            new AddNode(0, 0,
                                new IntegerNode(0, 0, -4),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "subtraction_with_negation_2"),
                            new SubtractNode(0, 0,
                                new IntegerNode(0, 0, 4),
                                new IntegerNode(0, 0, -2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "modulo_with_addition"),
                            new AddNode(0, 0,
                                new ModuloNode(0, 0,
                                    new IntegerNode(0, 0, 23),
                                    new IntegerNode(0, 0, 3)
                                ),
                                new IntegerNode(0, 0, 34)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "addition_with_modulo"),
                            new AddNode(0, 0,
                                new IntegerNode(0, 0, 23),
                                new ModuloNode(0, 0,
                                    new IntegerNode(0, 0, 9),
                                    new IntegerNode(0, 0, 3)
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "addition_with_multiplication"),
                            new AddNode(0, 0,
                                new RealNode(0, 0, 2.32),
                                new MultiplyNode(0, 0,
                                    new StringNode(0, 0, "hello"),
                                    new IdentifierNode(0, 0, "world")
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "division_with_or"),
                            new OrNode(0, 0,
                                new DivideNode(0, 0,
                                    new IntegerNode(0, 0, 4),
                                    new IntegerNode(0, 0, 2423)
                                ),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "expression_with_parenthesis"),
                            new MultiplyNode(0, 0,
                                new IntegerNode(0, 0, 23),
                                new SubtractNode(0, 0,
                                    new IntegerNode(0, 0, 4),
                                    new IntegerNode(0, 0, 5)
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "boolean_expression_with_and_and_or"),
                            new OrNode(0, 0,
                                new AndNode(0, 0,
                                    new IdentifierNode(0, 0, "hello"),
                                    new IdentifierNode(0, 0, "world")
                                ),
                                new IdentifierNode(0, 0, "foo")
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "boolean_expression_with_or_and_and"),
                            new OrNode(0, 0,
                                new IdentifierNode(0, 0, "hello"),
                                new AndNode(0, 0,
                                    new IdentifierNode(0, 0, "world"),
                                    new IdentifierNode(0, 0, "foo")
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "boolean_expression_with_and_and_not"),
                            new AndNode(0, 0,
                                new IdentifierNode(0, 0, "hello"),
                                new NotNode(0, 0,
                                    new IdentifierNode(0, 0, "world")
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "boolean_expression_with_not_and_or_with_parenthesis"),
                            new NotNode(0, 0,
                                new OrNode(0, 0,
                                    new IdentifierNode(0, 0, "hello"),
                                    new IdentifierNode(0, 0, "world")
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "equality_comparison"),
                            new EqualsNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "less_than"),
                            new LessThanNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new IntegerNode(0, 0, 7)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "less_or_equal"),
                            new LessThanOrEqualNode(0, 0,
                                new IntegerNode(0, 0, 7),
                                new IdentifierNode(0, 0, "b")
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "greater"),
                            new GreaterThanNode(0, 0,
                                new IntegerNode(0, 0, 4),
                                new IntegerNode(0, 0, 3)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "greater_or_equal"),
                            new GreaterThanOrEqualNode(0, 0,
                                new IntegerNode(0, 0, 4),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "not_equal"),
                            new NotEqualsNode(0, 0,
                                new IntegerNode(0, 0, 4),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "equality_with_expressions"),
                            new EqualsNode(0, 0,
                                new SubtractNode(0, 0,
                                    new IntegerNode(0, 0, 4),
                                    new IntegerNode(0, 0, 2)
                                ),
                                new AddNode(0, 0,
                                    new MultiplyNode(0, 0,
                                        new IntegerNode(0, 0, 2),
                                        new IntegerNode(0, 0, 3)
                                    ),
                                    new IntegerNode(0, 0, 4)
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "less_than_with_expressions"),
                            new LessThanNode(0, 0,
                                new SubtractNode(0, 0,
                                    new IntegerNode(0, 0, 4),
                                    new IntegerNode(0, 0, 2)
                                ),
                                new DivideNode(0, 0,
                                    new IntegerNode(0, 0, 2),
                                    new IntegerNode(0, 0, 3)
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "greater_than_with_expressions"),
                            new GreaterThanNode(0, 0,
                                new AndNode(0, 0,
                                    new NotNode(0, 0,
                                        new IdentifierNode(0, 0, "a")
                                    ),
                                    new IdentifierNode(0, 0, "b")
                                ),
                                new ModuloNode(0, 0,
                                    new IdentifierNode(0, 0, "a"),
                                    new RealNode(0, 0, 323.2)
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0, new IdentifierNode(0, 0, "less_or_equal_with_negation"),
                            new LessThanOrEqualNode(0, 0,
                                new IntegerNode(0, 0, -4),
                                new NegateNode(0, 0,
                                    new StringNode(0, 0, "abcdef")
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0,
                            new ArrayIndexNode(0, 0, new IdentifierToken("array_assignment"),
                                    new IntegerNode(0, 0, 2)
                                ),
                            new IntegerNode(0, 0, 2)
                        ),
                        new VariableAssignmentNode(0, 0,
                            new ArrayIndexNode(0, 0, new IdentifierToken("array_assignment_with_expression"),
                                    new AddNode(0, 0,
                                        new IntegerNode(0, 0, 5),
                                        new IntegerNode(0, 0, 2)
                                    )
                                ),
                            new IntegerNode(0, 0, 19)
                        ),
                        new VariableAssignmentNode(0, 0,
                            new ArrayIndexNode(0, 0, new IdentifierToken("array_assignment_with_boolean_expression"),
                                    new AndNode(0, 0,
                                        new IdentifierNode(0, 0, "hello"),
                                        new IntegerNode(0, 0, 452)
                                    )
                                ),
                            new IntegerNode(0, 0, 12)
                        )
                    )
                ),
            node);
        }

        [TestMethod()]
        public void InvalidExpressionsAreRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_expressions.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(17, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_expressions"),
                    new BlockNode(0, 0,
                        new ErrorNode(),
                        new ErrorNode(),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "c"),
                            new IntegerNode(0, 0, 1234)
                        ),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "h"),
                            new IntegerNode(0, 0, 244)
                        ),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "foo"),
                            new IntegerNode(0, 0, 4)
                        ),
                        new ErrorNode(),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "foo"),
                            new IntegerNode(0, 0, 5)
                        ),
                        new ErrorNode()
                    )
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(2, reporter.Errors[0].Line);
            Assert.AreEqual(18, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Unexpected token <operator - ';'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(3, reporter.Errors[1].Line);
            Assert.AreEqual(13, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Unexpected token <keyword - 'or'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(5, reporter.Errors[2].Line);
            Assert.AreEqual(17, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Expected token <identifier - 'size'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[3].Type);
            Assert.AreEqual(6, reporter.Errors[3].Line);
            Assert.AreEqual(17, reporter.Errors[3].Column);
            Assert.IsTrue(reporter.Errors[3].Message.Contains("Expected token <identifier>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[4].Type);
            Assert.AreEqual(7, reporter.Errors[4].Line);
            Assert.AreEqual(18, reporter.Errors[4].Column);
            Assert.IsTrue(reporter.Errors[4].Message.Contains("Unexpected token <operator - ';'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[5].Type);
            Assert.AreEqual(8, reporter.Errors[5].Line);
            Assert.AreEqual(13, reporter.Errors[5].Column);
            Assert.IsTrue(reporter.Errors[5].Message.Contains("Unexpected token <operator - '*'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[6].Type);
            Assert.AreEqual(9, reporter.Errors[6].Line);
            Assert.AreEqual(17, reporter.Errors[6].Column);
            Assert.IsTrue(reporter.Errors[6].Message.Contains("Expected token <operator - ';'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[7].Type);
            Assert.AreEqual(10, reporter.Errors[7].Line);
            Assert.AreEqual(14, reporter.Errors[7].Column);
            Assert.IsTrue(reporter.Errors[7].Message.Contains("Unexpected token <operator - '-'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[8].Type);
            Assert.AreEqual(11, reporter.Errors[8].Line);
            Assert.AreEqual(16, reporter.Errors[8].Column);
            Assert.IsTrue(reporter.Errors[8].Message.Contains("Unexpected token <operator - ';'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[9].Type);
            Assert.AreEqual(12, reporter.Errors[9].Line);
            Assert.AreEqual(13, reporter.Errors[9].Column);
            Assert.IsTrue(reporter.Errors[9].Message.Contains("Unexpected token <operator - '='> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[10].Type);
            Assert.AreEqual(13, reporter.Errors[10].Line);
            Assert.AreEqual(10, reporter.Errors[10].Column);
            Assert.IsTrue(reporter.Errors[10].Message.Contains("Expected token <operator - ':='>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[11].Type);
            Assert.AreEqual(14, reporter.Errors[11].Line);
            Assert.AreEqual(8, reporter.Errors[11].Column);
            Assert.IsTrue(reporter.Errors[11].Message.Contains("Unexpected token <operator - ':='>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[12].Type);
            Assert.AreEqual(15, reporter.Errors[12].Line);
            Assert.AreEqual(12, reporter.Errors[12].Column);
            Assert.IsTrue(reporter.Errors[12].Message.Contains("Expected token <operator - ']'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[13].Type);
            Assert.AreEqual(16, reporter.Errors[13].Line);
            Assert.AreEqual(9, reporter.Errors[13].Column);
            Assert.IsTrue(reporter.Errors[13].Message.Contains("Expected token <operator - ':='>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[14].Type);
            Assert.AreEqual(17, reporter.Errors[14].Line);
            Assert.AreEqual(18, reporter.Errors[14].Column);
            Assert.IsTrue(reporter.Errors[14].Message.Contains("Unexpected token <operator - ']'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[15].Type);
            Assert.AreEqual(22, reporter.Errors[15].Line);
            Assert.AreEqual(8, reporter.Errors[15].Column);
            Assert.IsTrue(reporter.Errors[15].Message.Contains("Expected token <operator - ';'> but was"));


            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[16].Type);
            Assert.AreEqual(29, reporter.Errors[16].Line);
            Assert.AreEqual(8, reporter.Errors[16].Column);
            Assert.IsTrue(reporter.Errors[16].Message.Contains("Expected token <operator - ';'> but was"));
        }

        [TestMethod()]
        public void ValidCallsAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_function_calls.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("valid_function_calls"),
                    new BlockNode(0, 0,
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "no_arguments")
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "one_argument"),
                            new IdentifierNode(0, 0, "hello")
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "one_argument_expression"),
                            new EqualsNode(0, 0,
                                new AddNode(0, 0,
                                    new IntegerNode(0, 0, 12),
                                    new IntegerNode(0, 0, 32)),
                                new SubtractNode(0, 0,
                                    new MultiplyNode(0, 0,
                                        new IntegerNode(0, 0, 1),
                                        new IntegerNode(0, 0, 2)
                                    ),
                                    new IntegerNode(0, 0, 4)
                                )
                            )
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "two_arguments"),
                            new IdentifierNode(0, 0, "hello"),
                            new IdentifierNode(0, 0, "world")
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "two_arguments_expressions"),
                            new MultiplyNode(0, 0,
                                new IntegerNode(0, 0, 23),
                                new RealNode(0, 0, 1.4312e2)
                            ),
                            new GreaterThanOrEqualNode(0, 0,
                                new StringNode(0, 0, "foo"),
                                new IdentifierNode(0, 0, "bar")
                            )
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "three_arguments"),
                            new IdentifierNode(0, 0, "hello"),
                            new IntegerNode(0, 0, 1),
                            new StringNode(0, 0, "world")
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "three_arguments_expressions"),
                            new AddNode(0, 0,
                                new IntegerNode(0, 0, 1),
                                new IntegerNode(0, 0, 2)
                            ),
                            new MultiplyNode(0, 0,
                                new IntegerNode(0, 0, 3),
                                new IntegerNode(0, 0, 1)
                            ),
                            new LessThanNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new AddNode(0, 0,
                                    new IdentifierNode(0, 0, "b"),
                                    new IdentifierNode(0, 0, "c")
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "a"),
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "hello")
                            )
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "b"),
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "hello"),
                                new IntegerNode(0, 0, 1),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new IfNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "foobar"),
                                new IntegerNode(0, 0, 32),
                                new IntegerNode(0, 0, 4)
                            ),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "q"),
                                    new IntegerNode(0, 0, 2)
                                )
                            )
                        )
                    )
                ),
                node);
        }

        [TestMethod()]
        public void InvalidCallsAreRejected()
        {

            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_function_calls.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(12, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_function_calls"),
                    new BlockNode(0, 0,
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "list_ending_in_comma"),
                            new IntegerNode(0, 0, 3),
                            new IntegerNode(0, 0, 4),
                            new ErrorNode()
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "invalid_expr"),
                            new ErrorNode()
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "invalid_expr2"),
                            new AddNode(0, 0,
                                new IntegerNode(0, 0, 3),
                                new IntegerNode(0, 0, 4)
                            ),
                            new ErrorNode()
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "a"),
                            new ErrorNode()
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "b"),
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "invalid_expression"),
                                new IntegerNode(0, 0, 3),
                                new ErrorNode()
                            )
                        ),
                        new IfNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "foo"),
                                new IntegerNode(0, 0, 3),
                                new ErrorNode()
                            ),
                            new BlockNode(0, 0,
                                 new VariableAssignmentNode(0, 0,
                                     new IdentifierNode(0, 0, "q"),
                                     new IntegerNode(0, 0, 4)
                                 )
                             )
                         ),
                         new WhileNode(0, 0,
                             new CallNode(0, 0,
                                new IdentifierNode(0, 0, "bar"),
                                new IntegerNode(0, 0, 3),
                                new IntegerNode(0, 0, 4),
                                new ErrorNode()
                             ),
                             new BlockNode(0, 0,
                                 new CallNode(0, 0,
                                     new IdentifierNode(0, 0, "foobar")
                                 )
                             )
                         )
                    )
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(2, reporter.Errors[0].Line);
            Assert.AreEqual(18, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Unexpected token <operator - ';'> when expression "));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(2, reporter.Errors[1].Line);
            Assert.AreEqual(18, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Expected token <operator - ')'> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(3, reporter.Errors[2].Line);
            Assert.AreEqual(17, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Expected token <operator - ':='>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[3].Type);
            Assert.AreEqual(4, reporter.Errors[3].Line);
            Assert.AreEqual(31, reporter.Errors[3].Column);
            Assert.IsTrue(reporter.Errors[3].Message.Contains("Expected token <operator - ')'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[4].Type);
            Assert.AreEqual(5, reporter.Errors[4].Line);
            Assert.AreEqual(35, reporter.Errors[4].Column);
            Assert.IsTrue(reporter.Errors[4].Message.Contains("Unexpected token <operator - ')'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[5].Type);
            Assert.AreEqual(6, reporter.Errors[5].Line);
            Assert.AreEqual(24, reporter.Errors[5].Column);
            Assert.IsTrue(reporter.Errors[5].Message.Contains("Unexpected token <operator - ','>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[6].Type);
            Assert.AreEqual(7, reporter.Errors[6].Line);
            Assert.AreEqual(29, reporter.Errors[6].Column);
            Assert.IsTrue(reporter.Errors[6].Message.Contains("Unexpected token <operator - '='>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[7].Type);
            Assert.AreEqual(9, reporter.Errors[7].Line);
            Assert.AreEqual(23, reporter.Errors[7].Column);
            Assert.IsTrue(reporter.Errors[7].Message.Contains("Unexpected token <operator - ';'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[8].Type);
            Assert.AreEqual(9, reporter.Errors[8].Line);
            Assert.AreEqual(23, reporter.Errors[8].Column);
            Assert.IsTrue(reporter.Errors[8].Message.Contains("Expected token <operator - ')'> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[9].Type);
            Assert.AreEqual(10, reporter.Errors[9].Line);
            Assert.AreEqual(38, reporter.Errors[9].Column);
            Assert.IsTrue(reporter.Errors[9].Message.Contains("Unexpected token <operator - ')'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[10].Type);
            Assert.AreEqual(12, reporter.Errors[10].Line);
            Assert.AreEqual(17, reporter.Errors[10].Column);
            Assert.IsTrue(reporter.Errors[10].Message.Contains("Unexpected token <operator - ')'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[11].Type);
            Assert.AreEqual(14, reporter.Errors[11].Line);
            Assert.AreEqual(24, reporter.Errors[11].Column);
            Assert.IsTrue(reporter.Errors[11].Message.Contains("Unexpected token <operator - ')'> when expression"));
        }

        [TestMethod()]
        public void ValidReturnStatementsAreAccepted()
        {

            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_return_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("return_statements"),
                    new BlockNode(0, 0,
                        new ReturnNode(0, 0),
                        new ReturnNode(0, 0,
                            new IntegerNode(0, 0, 5)
                        ),
                        new ReturnNode(0, 0,
                            new AddNode(0, 0,
                                new StringNode(0, 0, "hello"),
                                new IntegerNode(0, 0, 2)
                            )
                        ),
                        new ReturnNode(0, 0,
                            new LessThanOrEqualNode(0, 0,
                                new MultiplyNode(0, 0,
                                    new StringNode(0, 0, "hello"),
                                    new IntegerNode(0, 0, 2)
                                ),
                                new IntegerNode(0, 0, 4)
                            )
                        )
                    )
                ),
                node);
        }

        [TestMethod()]
        public void InvalidReturnStatementsAreRejected()
        {

            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_return_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(3, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_return_statements"),
                    new BlockNode(0, 0,
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ReturnNode(0, 0)
                    )
                ),
                node);


            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(2, reporter.Errors[0].Line);
            Assert.AreEqual(17, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Unexpected token <operator - ';'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(3, reporter.Errors[1].Line);
            Assert.AreEqual(15, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Unexpected token <operator - '>='> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(4, reporter.Errors[2].Line);
            Assert.AreEqual(21, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Expected token <operator - ')'>"));
        }

        [TestMethod()]
        public void ValidReadStatementsAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_read_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("read_statements"),
                    new BlockNode(0, 0,
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "read"),
                            new IdentifierNode(0, 0, "foo")
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "read"),
                            new IdentifierNode(0, 0, "foo"),
                            new IdentifierNode(0, 0, "bar")
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "read"),
                            new ArrayIndexNode(0, 0,
                                new IdentifierToken("foo"),
                                new IntegerNode(0, 0, 54)
                            ),
                            new ArrayIndexNode(0, 0,
                                new IdentifierToken("bar"),
                                new IntegerNode(0, 0, 12)
                            ),
                            new ArrayIndexNode(0, 0,
                                new IdentifierToken("baz"),
                                new SubtractNode(0, 0,
                                    new MultiplyNode(0, 0,
                                        new IntegerNode(0, 0, 12),
                                        new IntegerNode(0, 0, 2341)
                                    ),
                                    new IdentifierNode(0, 0, "qux")
                                )
                            )
                        )
                    )
                ),
                node
            );
        }

        [TestMethod()]
        public void ValidWriteStatementsAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_write_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("valid_write_statements"),
                    new BlockNode(0, 0,
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "writeln")
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "writeln"),
                            new IdentifierNode(0, 0, "hello")
                        ),
                        new CallNode(0, 0,
                            new IdentifierNode(0, 0, "writeln"),
                            new StringNode(0, 0, "foo"),
                            new IntegerNode(0, 0, 1),
                            new GreaterThanNode(0, 0,
                                new ModuloNode(0, 0,
                                    new IdentifierNode(0, 0, "a"),
                                    new IntegerNode(0, 0, 2)
                                ),
                                new NotNode(0, 0,
                                    new IntegerNode(0, 0, 4)
                                )
                            )
                        )
                    )
                ),
                node
            );
        }

        [TestMethod()]
        public void ValidAssertStatementsAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_assert_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("valid_assert_statements"),
                    new BlockNode(0, 0,
                        new AssertNode(0, 0,
                            new IdentifierNode(0, 0, "foo")
                        ),
                        new AssertNode(0, 0,
                            new GreaterThanOrEqualNode(0, 0,
                                new IdentifierNode(0, 0, "bar"),
                                new IntegerNode(0, 0, 3)
                            )
                        )
                    )
                ),
                node);
        }

        [TestMethod()]
        public void InvalidAssertStatementsAreRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_assert_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(3, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_assert_statements"),
                    new BlockNode(0, 0,
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode()
                    )
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(2, reporter.Errors[0].Line);
            Assert.AreEqual(15, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Unexpected token <operator - ')'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(3, reporter.Errors[1].Line);
            Assert.AreEqual(18, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Unexpected token <operator - ')'> when expression"));


            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(4, reporter.Errors[2].Line);
            Assert.AreEqual(18, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Expected token <operator - ')'>"));

        }

        [TestMethod()]
        public void ValidNesterBlocksAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_nested_blocks.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("valid_nested_blocks"),
                    new BlockNode(0, 0,
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "a"),
                            new IntegerNode(0, 0, 4)
                        ),
                        new BlockNode(0, 0,
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "inner_a"),
                                new IntegerNode(0, 0, 4)
                            ),
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "inner_b"),
                                new IntegerNode(0, 0, 6)
                            ),
                            new BlockNode(0, 0,
                                new CallNode(0, 0,
                                    new IdentifierNode(0, 0, "call"),
                                    new IntegerNode(0, 0, 23)
                                )
                            ),
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "another_call"),
                                new IntegerNode(0, 0, 1),
                                new IntegerNode(0, 0, 2122)
                            )
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "b"),
                            new IntegerNode(0, 0, 4)
                        )
                    )
                ),
                node);
        }

        [TestMethod()]
        public void InvalidNestedBlocksAreRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_nested_blocks.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(4, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_block_statements"),
                    new BlockNode(0, 0,
                        new BlockNode(0, 0,
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "abcd"),
                                new IntegerNode(0, 0, 12)
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new ErrorNode()
                        ),
                        new BlockNode(0, 0,
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new IntegerNode(0, 0, 322)
                            ),
                            new ErrorNode(),
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "b"),
                                new IntegerNode(0, 0, 4)
                            )

                        ),
                        new BlockNode(0, 0,
                            new ErrorNode()
                        )
                    )
                ),
                node);


            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(5, reporter.Errors[0].Line);
            Assert.AreEqual(8, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Expected token <operator - ';'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(9, reporter.Errors[1].Line);
            Assert.AreEqual(8, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Expected token <operator - ')'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(12, reporter.Errors[2].Line);
            Assert.AreEqual(25, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Expected token <operator - ';'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[3].Type);
            Assert.AreEqual(18, reporter.Errors[3].Line);
            Assert.AreEqual(8, reporter.Errors[3].Column);
            Assert.IsTrue(reporter.Errors[3].Message.Contains("Unexpected token <keyword - 'end'> when start of a"));
        }

        [TestMethod()]
        public void ValidIfStatementsAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_if_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("valid_if_statements"),
                    new BlockNode(0, 0,
                        new IfNode(0, 0,
                            new EqualsNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new IntegerNode(0, 0, 2323)
                            ),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new ArrayIndexNode(0, 0,
                                        new IdentifierToken("b"),
                                        new IntegerNode(0, 0, 4)
                                    ),
                                    new RealNode(0, 0, 4.335e6)
                                )
                            )
                        ),
                        new IfNode(0, 0,
                            new EqualsNode(0, 0,
                                new AddNode(0, 0,
                                    new IdentifierNode(0, 0, "b"),
                                    new StringNode(0, 0, " world")
                                ),
                                new StringNode(0, 0, "hello world")
                            ),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "foo"),
                                    new IntegerNode(0, 0, 246)
                                ),
                                new CallNode(0, 0,
                                    new IdentifierNode(0, 0, "abcdef"),
                                    new IntegerNode(0, 0, 432)
                                )
                            )
                        ),
                        new IfNode(0, 0,
                            new IdentifierNode(0, 0, "bar"),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "q"),
                                    new IntegerNode(0, 0, 23)
                                )
                            ),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "q"),
                                    new IntegerNode(0, 0, 32)
                                )
                            )
                        ),
                        new IfNode(0, 0,
                            new IdentifierNode(0, 0, "a"),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "foo"),
                                    new StringNode(0, 0, "hello")
                                ),
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "bar"),
                                    new IdentifierNode(0, 0, "world")
                                )
                            ),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "foo"),
                                    new StringNode(0, 0, "asdasd")
                                ),
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "bar"),
                                    new IntegerNode(0, 0, 42343124)
                                )
                            )
                        ),
                        new IfNode(0, 0,
                            new GreaterThanOrEqualNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new IntegerNode(0, 0, 4)
                            ),
                            new BlockNode(0, 0,
                                new IfNode(0, 0,
                                    new GreaterThanOrEqualNode(0, 0,
                                        new IdentifierNode(0, 0, "b"),
                                        new IntegerNode(0, 0, 5)
                                    ),
                                    new BlockNode(0, 0,
                                        new VariableAssignmentNode(0, 0,
                                            new IdentifierNode(0, 0, "q"),
                                            new IntegerNode(0, 0, 3)
                                        )
                                    ),
                                    new BlockNode(0, 0,
                                        new VariableAssignmentNode(0, 0,
                                           new IdentifierNode(0, 0, "a"),
                                           new IntegerNode(0, 0, 2)
                                        )
                                    )
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "foo"),
                            new IntegerNode(0, 0, 312)
                        ),
                        new IfNode(0, 0,
                            new GreaterThanOrEqualNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new IntegerNode(0, 0, 4)
                            ),
                            new BlockNode(0, 0,
                                new IfNode(0, 0,
                                    new GreaterThanOrEqualNode(0, 0,
                                        new IdentifierNode(0, 0, "b"),
                                        new IntegerNode(0, 0, 5)
                                    ),
                                    new BlockNode(0, 0,
                                        new VariableAssignmentNode(0, 0,
                                            new IdentifierNode(0, 0, "q"),
                                            new IntegerNode(0, 0, 3)
                                        )
                                    )
                                )
                            ),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "a"),
                                    new IntegerNode(0, 0, 2)
                                )
                            )
                        ),
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "bar"),
                            new IntegerNode(0, 0, 42)
                        )
                    )
                ),
                node);
        }

        [TestMethod()]
        public void InvalidIfStatementsAreRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_if_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(6, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_if_statements"),
                    new BlockNode(0, 0,
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new IfNode(0, 0,
                            new IdentifierNode(0, 0, "foo"),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "sda"),
                                    new RealNode(0, 0, 43432.1)
                                )
                            )
                        ),
                        new ErrorNode(),
                        new IfNode(0, 0,
                            new IdentifierNode(0, 0, "foo"),
                            new BlockNode(0, 0,
                                new ErrorNode()
                            )
                        ),
                        new IfNode(0, 0,
                            new IdentifierNode(0, 0, "foo"),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "q"),
                                    new IntegerNode(0, 0, 456)
                                )
                            ),
                            new BlockNode(0, 0,
                                new ErrorNode()
                            )

                        )
                    )
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(2, reporter.Errors[0].Line);
            Assert.AreEqual(29, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Unexpected token <keyword - 'then'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(5, reporter.Errors[1].Line);
            Assert.AreEqual(43, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Unexpected token <keyword - 'then'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(9, reporter.Errors[2].Line);
            Assert.AreEqual(16, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Expected token <keyword - 'then'> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[3].Type);
            Assert.AreEqual(12, reporter.Errors[3].Line);
            Assert.AreEqual(8, reporter.Errors[3].Column);
            Assert.IsTrue(reporter.Errors[3].Message.Contains("Unexpected token <keyword - 'else'> when start of a"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[4].Type);
            Assert.AreEqual(17, reporter.Errors[4].Line);
            Assert.AreEqual(8, reporter.Errors[4].Column);
            Assert.IsTrue(reporter.Errors[4].Message.Contains("Unexpected token <keyword - 'else'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[5].Type);
            Assert.AreEqual(23, reporter.Errors[5].Line);
            Assert.AreEqual(26, reporter.Errors[5].Column);
            Assert.IsTrue(reporter.Errors[5].Message.Contains("Unexpected token <operator - ';'> when expression"));
        }

        [TestMethod()]
        public void ValidWhileStatementsAreAccepted()
        {

            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_while_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("valid_while_statements"),
                    new BlockNode(0, 0,
                        new WhileNode(0, 0,
                            new IdentifierNode(0, 0, "true"),
                            new BlockNode(0, 0,
                                new CallNode(0, 0,
                                    new IdentifierNode(0, 0, "writeln"),
                                    new StringNode(0, 0, "abc_def")
                                )
                            )
                        ),
                        new WhileNode(0, 0,
                            new LessThanOrEqualNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new IntegerNode(0, 0, 6)
                            ),
                            new BlockNode(0, 0,
                                new IfNode(0, 0,
                                    new EqualsNode(0, 0,
                                        new ModuloNode(0, 0,
                                            new IdentifierNode(0, 0, "a"),
                                            new IntegerNode(0, 0, 2)
                                        ),
                                        new IntegerNode(0, 0, 0)
                                    ),
                                    new BlockNode(0, 0,
                                        new CallNode(0, 0,
                                            new IdentifierNode(0, 0, "writeln"),
                                            new StringNode(0, 0, "a: "),
                                            new IdentifierNode(0, 0, "a")
                                        )
                                    ),
                                    new BlockNode(0, 0,
                                        new CallNode(0, 0,
                                            new IdentifierNode(0, 0, "writeln"),
                                            new StringNode(0, 0, "")
                                        )
                                    )
                                )
                            )
                        ),
                        new WhileNode(0, 0,
                            new LessThanNode(0, 0,
                                new IdentifierNode(0, 0, "a"),
                                new IntegerNode(0, 0, 5)
                            ),
                            new BlockNode(0, 0,
                                new CallNode(0, 0,
                                    new IdentifierNode(0, 0, "writeln"),
                                    new StringNode(0, 0, "A: "),
                                    new IdentifierNode(0, 0, "a")
                                ),
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "a"),
                                    new AddNode(0, 0,
                                        new IdentifierNode(0, 0, "a"),
                                        new IntegerNode(0, 0, 1)
                                    )
                                )
                            )
                        )
                    )
                ),
                node);
        }

        [TestMethod()]
        public void InvalidWhileStatementsAreRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_while_statements.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(4, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_while_statements"),
                    new BlockNode(0, 0,
                        new ErrorNode(),
                        new ErrorNode(),
                        new WhileNode(0, 0,
                            new NotEqualsNode(0, 0,
                                new IdentifierNode(0, 0, "ads34"),
                                new RealNode(0, 0, 23.21)
                            ),
                            new BlockNode(0, 0,
                                new ErrorNode()
                            )
                        ),
                        new WhileNode(0, 0,
                            new IdentifierNode(0, 0, "foobar"),
                            new BlockNode(0, 0,
                                new CallNode(0, 0,
                                    new IdentifierNode(0, 0, "call_foo")
                                ),
                                new ErrorNode()
                            )
                        )
                    )
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(2, reporter.Errors[0].Line);
            Assert.AreEqual(10, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Unexpected token <keyword - 'do'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(6, reporter.Errors[1].Line);
            Assert.AreEqual(16, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Expected token <keyword - 'do'> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(9, reporter.Errors[2].Line);
            Assert.AreEqual(26, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Unexpected token <operator - '-'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[3].Type);
            Assert.AreEqual(13, reporter.Errors[3].Line);
            Assert.AreEqual(26, reporter.Errors[3].Column);
            Assert.IsTrue(reporter.Errors[3].Message.Contains("Expected token <operator - ';'> but was"));
        }

        [TestMethod()]
        public void ValidVariableDeclarationsAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_variable_declarations.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);

            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("valid_variable_declarations"),
                    new BlockNode(0, 0,
                        new VariableDeclarationNode(0, 0,
                            new IdentifierNode(0, 0, "integer"),
                            new IdentifierNode(0, 0, "a")
                        ),
                        new VariableDeclarationNode(0, 0,
                            new IdentifierNode(0, 0, "string"),
                            new IdentifierNode(0, 0, "b"),
                            new IdentifierNode(0, 0, "c")
                        ),
                        new VariableDeclarationNode(0, 0,
                            new IdentifierNode(0, 0, "boolean"),
                            new IdentifierNode(0, 0, "d")
                        ),
                        new VariableDeclarationNode(0, 0,
                            new IdentifierNode(0, 0, "real"),
                            new IdentifierNode(0, 0, "e"),
                            new IdentifierNode(0, 0, "f"),
                            new IdentifierNode(0, 0, "g"),
                            new IdentifierNode(0, 0, "h")
                        ),
                        new VariableDeclarationNode(0, 0,
                            new ArrayTypeNode(0, 0,
                                new IdentifierNode(0, 0, "integer"),
                                new IntegerNode(0, 0, 5)
                            ),
                            new IdentifierNode(0, 0, "i"),
                            new IdentifierNode(0, 0, "j"),
                            new IdentifierNode(0, 0, "k")
                        ),
                        new VariableDeclarationNode(0, 0,
                            new ArrayTypeNode(0, 0,
                                new IdentifierNode(0, 0, "string"),
                                new AddNode(0, 0,
                                    new IdentifierNode(0, 0, "foo"),
                                    new IntegerNode(0, 0, 32)
                                )
                            ),
                            new IdentifierNode(0, 0, "l")
                        ),
                        new VariableDeclarationNode(0, 0,
                            new ArrayTypeNode(0, 0,
                                new IdentifierNode(0, 0, "real"),
                                new NotEqualsNode(0, 0,
                                    new MultiplyNode(0, 0,
                                        new AddNode(0, 0,
                                            new IntegerNode(0, 0, 8),
                                            new IntegerNode(0, 0, 4)
                                        ),
                                        new IdentifierNode(0, 0, "bar")
                                    ),
                                    new IntegerNode(0, 0, 2)
                                )
                            ),
                            new IdentifierNode(0, 0, "m")
                        ),
                        new VariableDeclarationNode(0, 0,
                            new ArrayTypeNode(0, 0,
                                new IdentifierNode(0, 0, "boolean"),
                                new AddNode(0, 0,
                                    new StringNode(0, 0, "invalid_semantically"),
                                    new IntegerNode(0, 0, 23)
                                )
                            ),
                            new IdentifierNode(0, 0, "n"),
                            new IdentifierNode(0, 0, "o")
                        )
                    )
                ),
            node);
        }

        [TestMethod()]
        public void InvalidVariableDeclarationsAreRejected()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_variable_declarations.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(13, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_variable_declarations"),
                    new BlockNode(0, 0,
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode(),
                        new ErrorNode()
                    )
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(2, reporter.Errors[0].Line);
            Assert.AreEqual(12, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Expected token <identifier> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(3, reporter.Errors[1].Line);
            Assert.AreEqual(14, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Expected token <operator - ':'> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(4, reporter.Errors[2].Line);
            Assert.AreEqual(16, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Expected token <identifier> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[3].Type);
            Assert.AreEqual(5, reporter.Errors[3].Line);
            Assert.AreEqual(16, reporter.Errors[3].Column);
            Assert.IsTrue(reporter.Errors[3].Message.Contains("'not_a_valid_type' is not a valid type"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[4].Type);
            Assert.AreEqual(6, reporter.Errors[4].Line);
            Assert.AreEqual(15, reporter.Errors[4].Column);
            Assert.IsTrue(reporter.Errors[4].Message.Contains("Expected token <identifier> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[5].Type);
            Assert.AreEqual(7, reporter.Errors[5].Line);
            Assert.AreEqual(14, reporter.Errors[5].Column);
            Assert.IsTrue(reporter.Errors[5].Message.Contains("Expected token <operator - ':'> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[6].Type);
            Assert.AreEqual(8, reporter.Errors[6].Line);
            Assert.AreEqual(16, reporter.Errors[6].Column);
            Assert.IsTrue(reporter.Errors[6].Message.Contains("'rray' is not a valid"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[7].Type);
            Assert.AreEqual(9, reporter.Errors[7].Line);
            Assert.AreEqual(22, reporter.Errors[7].Column);
            Assert.IsTrue(reporter.Errors[7].Message.Contains("Expected token <operator - '['> but was"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[8].Type);
            Assert.AreEqual(10, reporter.Errors[8].Line);
            Assert.AreEqual(27, reporter.Errors[8].Column);
            Assert.IsTrue(reporter.Errors[8].Message.Contains("Unexpected token <operator - ']'> when expression"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[9].Type);
            Assert.AreEqual(11, reporter.Errors[9].Line);
            Assert.AreEqual(26, reporter.Errors[9].Column);
            Assert.IsTrue(reporter.Errors[9].Message.Contains("Expected token <operator - ']'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[10].Type);
            Assert.AreEqual(12, reporter.Errors[10].Line);
            Assert.AreEqual(27, reporter.Errors[10].Column);
            Assert.IsTrue(reporter.Errors[10].Message.Contains("Expected token <keyword - 'of'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[11].Type);
            Assert.AreEqual(13, reporter.Errors[11].Line);
            Assert.AreEqual(29, reporter.Errors[11].Column);
            Assert.IsTrue(reporter.Errors[11].Message.Contains("Expected token <identifier>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[12].Type);
            Assert.AreEqual(14, reporter.Errors[12].Line);
            Assert.AreEqual(30, reporter.Errors[12].Column);
            Assert.IsTrue(reporter.Errors[12].Message.Contains("'foobar' is not a valid"));
        }

        [TestMethod()]
        public void ParserAcceptsValidProcedureDeclarations()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_procedure_declarations.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("valid_procedure_declarations"),
                    new BlockNode(0, 0,
                        new ProcedureNode(0, 0,
                            new IdentifierNode(0, 0, "foo"),
                            new BlockNode(0, 0,
                                new VariableAssignmentNode(0, 0,
                                    new IdentifierNode(0, 0, "q"),
                                    new IntegerNode(0, 0, 343)
                                ),
                                new ReturnNode(0, 0)
                            )
                        ),
                        new ProcedureNode(0, 0,
                            new IdentifierNode(0, 0, "bar"),
                            new BlockNode(0, 0,
                                new ReturnNode(0, 0,
                                    new AddNode(0, 0,
                                        new IntegerNode(0, 0, 1),
                                        new IntegerNode(0, 0, 2)
                                    )
                                )
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "integer"),
                                new IdentifierNode(0, 0, "a")
                            )
                        ),
                        new ProcedureNode(0, 0,
                            new IdentifierNode(0, 0, "baz"),
                            new BlockNode(0, 0,
                                new ReturnNode(0, 0,
                                    new AddNode(0, 0,
                                        new IntegerNode(0, 0, 1),
                                        new IntegerNode(0, 0, 2)
                                    )
                                )
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "integer"),
                                new IdentifierNode(0, 0, "a")
                            ),
                            new VariableDeclarationNode(0, 0,
                                new ArrayTypeNode(0, 0,
                                    new IdentifierNode(0, 0, "integer"),
                                    new IntegerNode(0, 0, 4)
                                ),
                                new IdentifierNode(0, 0, "b")
                            )

                        ),
                        new ProcedureNode(0, 0,
                            new IdentifierNode(0, 0, "qux"),
                            new BlockNode(0, 0,
                                new ReturnNode(0, 0)
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "integer"),
                                new IdentifierNode(0, 0, "a")
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "boolean"),
                                new IdentifierNode(0, 0, "b")
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "real"),
                                new IdentifierNode(0, 0, "c")
                            )

                        )
                    )
                ),
                node);
        }

        [TestMethod()]
        public void ParserRejectsInvalidProcedureDeclarations()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_procedure_declarations.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(10, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_procedure_declarations"),
                    new BlockNode(0, 0,
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new VariableAssignmentNode(0, 0, 
                                new IdentifierNode(0, 0, "q"),
                                new IntegerNode(0, 0, 343)
                            ),
                            new ReturnNode(0, 0)
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "abc"),
                                new IntegerNode(0, 0, 132)
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "qux"),
                                new IntegerNode(0, 0, 743)
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "ttt"),
                                new IntegerNode(0, 0, 333)
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "vcx"),
                                new IntegerNode(0, 0, 34523)
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new VariableAssignmentNode(0, 0,
                                new IdentifierNode(0, 0, "bvcx"),
                                new IntegerNode(0, 0, 0)
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "q___"),
                                new IntegerNode(0, 0, 0)
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "x")
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "abcde")
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "vfr")
                            )
                        )
                    )
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(2, reporter.Errors[0].Line);
            Assert.AreEqual(18, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Expected token <identifier>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(8, reporter.Errors[1].Line);
            Assert.AreEqual(24, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Expected token <operator - '('>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(13, reporter.Errors[2].Line);
            Assert.AreEqual(25, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Expected token <identifier>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[3].Type);
            Assert.AreEqual(18, reporter.Errors[3].Line);
            Assert.AreEqual(25, reporter.Errors[3].Column);
            Assert.IsTrue(reporter.Errors[3].Message.Contains("Expected token <identifier>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[4].Type);
            Assert.AreEqual(23, reporter.Errors[4].Line);
            Assert.AreEqual(29, reporter.Errors[4].Column);
            Assert.IsTrue(reporter.Errors[4].Message.Contains("Expected token <identifier>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[5].Type);
            Assert.AreEqual(28, reporter.Errors[5].Line);
            Assert.AreEqual(29, reporter.Errors[5].Column);
            Assert.IsTrue(reporter.Errors[5].Message.Contains("Expected token <operator - ':'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[6].Type);
            Assert.AreEqual(33, reporter.Errors[6].Line);
            Assert.AreEqual(31, reporter.Errors[6].Column);
            Assert.IsTrue(reporter.Errors[6].Message.Contains("'bar' is not a valid type"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[7].Type);
            Assert.AreEqual(38, reporter.Errors[7].Line);
            Assert.AreEqual(31, reporter.Errors[7].Column);
            Assert.IsTrue(reporter.Errors[7].Message.Contains("Expected token <identifier>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[8].Type);
            Assert.AreEqual(43, reporter.Errors[8].Line);
            Assert.AreEqual(40, reporter.Errors[8].Column);
            Assert.IsTrue(reporter.Errors[8].Message.Contains("Expected token <identifier>"));

        }

        /*
                 program valid_function_declarations;
        begin

            function foobar() : integer;
	begin
        a()

    end;

	function foobar(var a : integer, var b : real, var c : string) : real;
	begin
        b()

    end;

	function foobar(a : integer, b : real, c : string) : BOOLEAN;
	begin
        c()

    end;

	function foobar(a : integer, var b : real, c : string) : string;
	begin
        d()

    end;


end.
             
             */

        [TestMethod()]
        public void ParserAcceptsValidFunctionDeclarations()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\valid_function_declarations.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(0, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("valid_function_declarations"),
                    new BlockNode(0, 0,
                        new FunctionNode(0, 0,
                            new IdentifierNode(0, 0, "foobar"),
                            new IdentifierNode(0, 0, "integer"),
                            new BlockNode(0, 0,
                                new CallNode(0, 0,
                                    new IdentifierNode(0, 0, "a")
                                )
                            )
                        ),
                        new FunctionNode(0, 0,
                            new IdentifierNode(0, 0, "bar"),
                            new IdentifierNode(0, 0, "real"),
                            new BlockNode(0, 0,
                                new CallNode(0, 0,
                                    new IdentifierNode(0, 0, "b")
                                )
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "integer"),
                                new IdentifierNode(0, 0, "a")
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "real"),
                                new IdentifierNode(0, 0, "b")
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "string"),
                                new IdentifierNode(0, 0, "c")
                            )
                        ),
                        new FunctionNode(0, 0,
                            new IdentifierNode(0, 0, "baz"),
                            new IdentifierNode(0, 0, "boolean"),
                            new BlockNode(0, 0,
                                new CallNode(0, 0,
                                    new IdentifierNode(0, 0, "c")
                                )
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "integer"),
                                new IdentifierNode(0, 0, "a")
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "real"),
                                new IdentifierNode(0, 0, "b")
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "string"),
                                new IdentifierNode(0, 0, "c")
                            )
                        ),
                        new FunctionNode(0, 0,
                            new IdentifierNode(0, 0, "qux"),
                            new IdentifierNode(0, 0, "string"),
                            new BlockNode(0, 0,
                                new CallNode(0, 0,
                                    new IdentifierNode(0, 0, "d")
                                )
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "integer"),
                                new IdentifierNode(0, 0, "a")
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "real"),
                                new IdentifierNode(0, 0, "b")
                            ),
                            new VariableDeclarationNode(0, 0,
                                new IdentifierNode(0, 0, "string"),
                                new IdentifierNode(0, 0, "c")
                            )
                        )
                    )
                ),
                node);
        }

        [TestMethod()]
        public void ParserRejectsInvalidFunctionDeclarations()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_function_declarations.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(4, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("invalid_function_declarations"),
                    new BlockNode(0, 0,
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "a")
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "b")
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "c")
                            )
                        ),
                        new ErrorNode(),
                        new BlockNode(0, 0,
                            new CallNode(0, 0,
                                new IdentifierNode(0, 0, "d")
                            )
                        )
                    )
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(3, reporter.Errors[0].Line);
            Assert.AreEqual(22, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Expected token <identifier>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(8, reporter.Errors[1].Line);
            Assert.AreEqual(23, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Expected token <operator - ':'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(13, reporter.Errors[2].Line);
            Assert.AreEqual(43, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("'abcdef' is not a valid"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[3].Type);
            Assert.AreEqual(18, reporter.Errors[3].Line);
            Assert.AreEqual(25, reporter.Errors[3].Column);
            Assert.IsTrue(reporter.Errors[3].Message.Contains("Expected token <identifier>"));
        }

        [TestMethod()]
        public void ParserRejectsProgramWithMissingEndDot()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_program_with_missing_end_dot.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(2, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("foo"),
                    new ErrorNode()
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(4, reporter.Errors[0].Line);
            Assert.AreEqual(10, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Expected token <keyword - 'end'>"));

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(4, reporter.Errors[1].Line);
            Assert.AreEqual(10, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Expected token <operator - '.'>"));
        }

        [TestMethod()]
        public void TokensAfterEndOfProgramAreError()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Parsing\invalid_tokens_after_the_end_of_program.txt", reporter);
            var parser = new Parser(lexer, reporter);

            var node = parser.Parse();

            Assert.AreEqual(1, reporter.Errors.Count);
            ASTMatches(
                new ProgramNode(0, 0, new IdentifierToken("tokens_after_program"),
                    new BlockNode(0, 0,
                        new VariableAssignmentNode(0, 0,
                            new IdentifierNode(0, 0, "a"),
                            new IntegerNode(0, 0, 4)
                        )
                    )
                ),
                node);

            Assert.AreEqual(Error.SYNTAX_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(7, reporter.Errors[0].Line);
            Assert.AreEqual(0, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Unexpected token <identifier - 'call'> after the end"));
        }

        void ASTMatches(ASTNode expected, ASTNode actual)
        {
            var expectedStack = new Stack<ASTNode>();
            var actualStack = new Stack<ASTNode>();

            expectedStack.Push(expected);
            actualStack.Push(actual);

            while (expectedStack.Count != 0)
            {
                Assert.IsTrue(actualStack.Count != 0);

                var currentExpected = expectedStack.Pop();
                var currentActual = actualStack.Pop();

                Assert.AreEqual(currentExpected, currentActual);
                Assert.AreEqual(currentExpected.Children.Count, currentActual.Children.Count);

                for (int i = currentExpected.Children.Count - 1; i >= 0; --i)
                {
                    expectedStack.Push(currentExpected.Children[i]);
                }

                for (int i = currentActual.Children.Count - 1; i >= 0; --i)
                {
                    actualStack.Push(currentActual.Children[i]);
                }
            }

            Assert.AreEqual(0, expectedStack.Count);
            Assert.AreEqual(0, actualStack.Count);
        }
    }
}