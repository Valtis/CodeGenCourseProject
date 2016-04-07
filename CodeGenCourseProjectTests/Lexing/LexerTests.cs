using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenCourseProject.Tokens;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;

namespace CodeGenCourseWork.Lexing.Tests
{
    [TestClass()]
    public class LexerTests
    {
        [TestMethod()]
        public void ValidIdentifiersAreAccepted()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\valid_identifiers.txt", reporter);

            Assert.AreEqual(new IdentifierToken("i"), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("id"), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("idw1with2numbers"), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("id_with_underscores"), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("id_with123_numbers_and_underscores"), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("ends_in_underscore_"), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("ends_in_number123"), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("functionnotkeyword"), lexer.NextToken());
            Assert.AreEqual(new EOFToken(), lexer.NextToken());
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidKeywordsAreHandled()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\valid_keywords.txt", reporter);

            Assert.AreEqual(new OrToken(), lexer.NextToken());
            Assert.AreEqual(new AndToken(), lexer.NextToken());
            Assert.AreEqual(new NotToken(), lexer.NextToken());
            Assert.AreEqual(new IfToken(), lexer.NextToken());
            Assert.AreEqual(new ThenToken(), lexer.NextToken());
            Assert.AreEqual(new ElseToken(), lexer.NextToken());
            Assert.AreEqual(new OfToken(), lexer.NextToken());
            Assert.AreEqual(new WhileToken(), lexer.NextToken());
            Assert.AreEqual(new DoToken(), lexer.NextToken());
            Assert.AreEqual(new BeginToken(), lexer.NextToken());
            Assert.AreEqual(new EndToken(), lexer.NextToken());
            Assert.AreEqual(new VarToken(), lexer.NextToken());
            Assert.AreEqual(new ArrayToken(), lexer.NextToken());
            Assert.AreEqual(new ProcedureToken(), lexer.NextToken());
            Assert.AreEqual(new FunctionToken(), lexer.NextToken());
            Assert.AreEqual(new ProgramToken(), lexer.NextToken());
            Assert.AreEqual(new AssertToken(), lexer.NextToken());
            Assert.AreEqual(new EOFToken(), lexer.NextToken());
            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidOperatorsAreHandled()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\valid_operators.txt", reporter);

            Assert.AreEqual(new PlusToken(), lexer.NextToken());
            Assert.AreEqual(new MinusToken(), lexer.NextToken());
            Assert.AreEqual(new MultiplyToken(), lexer.NextToken());
            Assert.AreEqual(new DivideToken(), lexer.NextToken());
            Assert.AreEqual(new ModuloToken(), lexer.NextToken());
            Assert.AreEqual(new EqualsToken(), lexer.NextToken());
            Assert.AreEqual(new NotEqualsToken(), lexer.NextToken());
            Assert.AreEqual(new LessThanToken(), lexer.NextToken());
            Assert.AreEqual(new GreaterThanToken(), lexer.NextToken());
            Assert.AreEqual(new LessThanOrEqualToken(), lexer.NextToken());
            Assert.AreEqual(new GreaterThanOrEqualToken(), lexer.NextToken());
            Assert.AreEqual(new LParenToken(), lexer.NextToken());
            Assert.AreEqual(new RParenToken(), lexer.NextToken());
            Assert.AreEqual(new LBracketToken(), lexer.NextToken());
            Assert.AreEqual(new RBracketToken(), lexer.NextToken());
            Assert.AreEqual(new AssignmentToken(), lexer.NextToken());
            Assert.AreEqual(new PeriodToken(), lexer.NextToken());
            Assert.AreEqual(new CommaToken(), lexer.NextToken());
            Assert.AreEqual(new SemicolonToken(), lexer.NextToken());
            Assert.AreEqual(new ColonToken(), lexer.NextToken());
            Assert.AreEqual(new MultiplyToken(), lexer.NextToken());
            Assert.AreEqual(new MultiplyToken(), lexer.NextToken());
            Assert.AreEqual(new MultiplyToken(), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("hello"), lexer.NextToken());
            Assert.AreEqual(new PlusToken(), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("world"), lexer.NextToken());
            Assert.AreEqual(new DivideToken(), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(123), lexer.NextToken());
            Assert.AreEqual(new EOFToken(), lexer.NextToken());

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidNumbersAreHandled()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\valid_numbers.txt", reporter);
            
            Assert.AreEqual(new IntegerToken(1234), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(12345568), lexer.NextToken());
            Assert.AreEqual(new RealToken(987.654), lexer.NextToken());
            Assert.AreEqual(new RealToken(765.23e12), lexer.NextToken());
            Assert.AreEqual(new RealToken(731.3e-92), lexer.NextToken());
            Assert.AreEqual(new EOFToken(), lexer.NextToken());

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidNumbersAreHandled()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\invalid_numbers.txt", reporter);

            Assert.AreEqual(new IntegerToken(1), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(123), lexer.NextToken());
            Assert.AreEqual(new RealToken(423.423121), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(456), lexer.NextToken());
            Assert.AreEqual(new RealToken(123), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(789), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(3216), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(101112), lexer.NextToken());
            Assert.AreEqual(new RealToken(1.235e33), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(987), lexer.NextToken());
            Assert.AreEqual(new RealToken(1.23e27), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(654), lexer.NextToken());
            Assert.AreEqual(new RealToken(1.2323E+64), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(7531), lexer.NextToken());
            Assert.AreEqual(new RealToken(1), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(5343), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("x"), lexer.NextToken());
            Assert.AreEqual(new RealToken(1.234e46), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("ybbb"), lexer.NextToken());
            Assert.AreEqual(new EOFToken(), lexer.NextToken());

            Assert.AreEqual(10, reporter.Errors.Count);


            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(0, reporter.Errors[0].Line);
            Assert.AreEqual(0, reporter.Errors[0].Column);
            Assert.AreEqual("Number does not fit 32 bit signed integer", reporter.Errors[0].Message);
            
            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(2, reporter.Errors[1].Line);
            Assert.AreEqual(7, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Erroneus extra decimal"));
            
            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[2].Type);
            Assert.AreEqual(4, reporter.Errors[2].Line);
            Assert.AreEqual(3, reporter.Errors[2].Column);
            Assert.IsTrue(reporter.Errors[2].Message.Contains("Decimal separator must be"));

            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[3].Type);
            Assert.AreEqual(6, reporter.Errors[3].Line);
            Assert.AreEqual(3, reporter.Errors[3].Column);
            Assert.IsTrue(reporter.Errors[3].Message.Contains("Integer cannot contain"));

            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[4].Type);
            Assert.AreEqual(8, reporter.Errors[4].Line);
            Assert.AreEqual(7, reporter.Errors[4].Column);
            Assert.IsTrue(reporter.Errors[4].Message.Contains("Real number cannot contain more"));

            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[5].Type);
            Assert.AreEqual(10, reporter.Errors[5].Line);
            Assert.AreEqual(3, reporter.Errors[5].Column);
            Assert.IsTrue(reporter.Errors[5].Message.Contains("Decimal separator must be"));

            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[6].Type);
            Assert.AreEqual(12, reporter.Errors[6].Line);
            Assert.AreEqual(8, reporter.Errors[6].Column);
            Assert.IsTrue(reporter.Errors[6].Message.Contains("Erroneus extra decimal"));

            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[7].Type);
            Assert.AreEqual(14, reporter.Errors[7].Line);
            Assert.AreEqual(0, reporter.Errors[7].Column);
            Assert.AreEqual("Number does not fit 64 bit real", reporter.Errors[7].Message);

            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[8].Type);
            Assert.AreEqual(15, reporter.Errors[8].Line);
            Assert.AreEqual(4, reporter.Errors[8].Column);
            Assert.IsTrue(reporter.Errors[8].Message.Contains("Invalid character 'x'"));

            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[9].Type);
            Assert.AreEqual(16, reporter.Errors[9].Line);
            Assert.AreEqual(8, reporter.Errors[9].Column);
            Assert.IsTrue(reporter.Errors[9].Message.Contains("Invalid character 'y'"));
        }

        [TestMethod()]
        public void ValidCommentsAreHandled()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\valid_comments.txt", reporter);

            Assert.AreEqual(new VarToken(), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("a"), lexer.NextToken());
            Assert.AreEqual(new ColonToken(), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("integer"), lexer.NextToken());
            Assert.AreEqual(new SemicolonToken(), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("foobar"), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("poitot"), lexer.NextToken());
            Assert.AreEqual(new EOFToken(), lexer.NextToken());

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void InvalidCommentsAreHandled()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\invalid_comments.txt", reporter);

            Assert.AreEqual(new PlusToken(), lexer.NextToken());
            Assert.AreEqual(new IdentifierToken("not_a_comment"), lexer.NextToken());
            Assert.AreEqual(new EOFToken(), lexer.NextToken());

            Assert.AreEqual(2, reporter.Errors.Count);


            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[0].Type);
            Assert.AreEqual(0, reporter.Errors[0].Line);
            Assert.AreEqual(1, reporter.Errors[0].Column);
            Assert.IsTrue(reporter.Errors[0].Message.Contains("Unexpected character '+'"));


            Assert.AreEqual(Error.LEXICAL_ERROR, reporter.Errors[1].Type);
            Assert.AreEqual(4, reporter.Errors[1].Line);
            Assert.AreEqual(26, reporter.Errors[1].Column);
            Assert.IsTrue(reporter.Errors[1].Message.Contains("Unexpected end of file"));
        }

        [TestMethod()]
        public void IntendationIsHandledCorrectlyWhenTabIs8Char()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\indentation.txt", reporter, 8);


            Token token;

            token = lexer.NextToken(); 
            Assert.AreEqual(token, new IdentifierToken("no_indentation"));
            Assert.AreEqual(0, token.Line);
            Assert.AreEqual(0, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_4_spaces"));
            Assert.AreEqual(1, token.Line);
            Assert.AreEqual(4, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_8_spaces"));
            Assert.AreEqual(2, token.Line);
            Assert.AreEqual(8, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("one_tab"));
            Assert.AreEqual(3, token.Line);
            Assert.AreEqual(8, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_2_spaces_and_tab"));
            Assert.AreEqual(4, token.Line);
            Assert.AreEqual(8, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_4_spaces_and_tab"));
            Assert.AreEqual(5, token.Line);
            Assert.AreEqual(8, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_one_space_and_tab"));
            Assert.AreEqual(6, token.Line);
            Assert.AreEqual(8, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("tab_space_tab"));
            Assert.AreEqual(7, token.Line);
            Assert.AreEqual(16, token.Column);
            
            Assert.AreEqual(new EOFToken(), lexer.NextToken());

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void IntendationIsHandledCorrectlyWhenTabIs4Char()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\indentation.txt", reporter, 4);

            Token token;

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("no_indentation"));
            Assert.AreEqual(0, token.Line);
            Assert.AreEqual(0, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_4_spaces"));
            Assert.AreEqual(1, token.Line);
            Assert.AreEqual(4, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_8_spaces"));
            Assert.AreEqual(2, token.Line);
            Assert.AreEqual(8, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("one_tab"));
            Assert.AreEqual(3, token.Line);
            Assert.AreEqual(4, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_2_spaces_and_tab"));
            Assert.AreEqual(4, token.Line);
            Assert.AreEqual(4, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_4_spaces_and_tab"));
            Assert.AreEqual(5, token.Line);
            Assert.AreEqual(8, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("x_one_space_and_tab"));
            Assert.AreEqual(6, token.Line);
            Assert.AreEqual(4, token.Column);

            token = lexer.NextToken();
            Assert.AreEqual(token, new IdentifierToken("tab_space_tab"));
            Assert.AreEqual(7, token.Line);
            Assert.AreEqual(8, token.Column);

            Assert.AreEqual(new EOFToken(), lexer.NextToken());

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void ValidStringsAreHandled()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\valid_strings.txt", reporter);

            Assert.AreEqual(new StringToken("hello"), lexer.NextToken());
            Assert.AreEqual(new StringToken("valid_string\n"), lexer.NextToken());
            Assert.AreEqual(new StringToken("\tanother\"valid_string\r\\"), lexer.NextToken());
            Assert.AreEqual(new EOFToken(), lexer.NextToken());

            Assert.AreEqual(0, reporter.Errors.Count);
        }

        [TestMethod()]
        public void BacktrackingPeekingAndNextWorks()
        {
            var reporter = new ErrorReporter();
            var lexer = new Lexer(@"..\..\Lexing\backtracking.txt", reporter);

            Assert.AreEqual(new IntegerToken(1), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(2), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(3), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(4), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(5), lexer.PeekToken());
            Assert.AreEqual(new IntegerToken(5), lexer.PeekToken());
            lexer.Backtrack();
            Assert.AreEqual(new IntegerToken(4), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(5), lexer.PeekToken());
            Assert.AreEqual(new IntegerToken(5), lexer.PeekToken());
            Assert.AreEqual(new IntegerToken(5), lexer.NextToken());

            lexer.Backtrack();
            lexer.Backtrack();
            lexer.Backtrack();
            lexer.Backtrack();
            Assert.AreEqual(new IntegerToken(2), lexer.PeekToken());
            Assert.AreEqual(new IntegerToken(2), lexer.PeekToken());
            Assert.AreEqual(new IntegerToken(2), lexer.PeekToken());
            Assert.AreEqual(new IntegerToken(2), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(3), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(4), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(5), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(6), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(7), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(8), lexer.NextToken());
            Assert.AreEqual(new IntegerToken(9), lexer.PeekToken());
            Assert.AreEqual(new IntegerToken(9), lexer.PeekToken());
            Assert.AreEqual(new IntegerToken(9), lexer.PeekToken());
            Assert.AreEqual(0, reporter.Errors.Count);
        }
    }
}