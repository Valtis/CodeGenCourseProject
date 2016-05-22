using System;
using System.Collections.Generic;
using CodeGenCourseProject.Tokens;
using CodeGenCourseProject.ErrorHandling;

namespace CodeGenCourseProject.Lexing
{
    internal class OperatorScanner : TokenScanner
    {
        private IDictionary<string, Type> operators;

        /*
        Parses operator tokens 
        */
        internal OperatorScanner(TextReader reader, MessageReporter reporter) : base(reader, reporter)
        {
            operators = new Dictionary<string, Type>();

            operators.Add("+", typeof(PlusToken));
            operators.Add("-", typeof(MinusToken));
            operators.Add("*", typeof(MultiplyToken));
            operators.Add("/", typeof(DivideToken));
            operators.Add("%", typeof(ModuloToken));
            operators.Add("=", typeof(EqualsToken));
            operators.Add("<>", typeof(NotEqualsToken));
            operators.Add("<", typeof(LessThanToken));
            operators.Add(">", typeof(GreaterThanToken));
            operators.Add("<=", typeof(LessThanOrEqualToken));
            operators.Add(">=", typeof(GreaterThanOrEqualToken));
            operators.Add("(", typeof(LParenToken));
            operators.Add(")", typeof(RParenToken));
            operators.Add("[", typeof(LBracketToken));
            operators.Add("]", typeof(RBracketToken));
            operators.Add(":=", typeof(AssignmentToken));
            operators.Add(".", typeof(PeriodToken));
            operators.Add(",", typeof(CommaToken)); 
            // semicolon isn't really an operator, but we treat it as such for simplicity 
            operators.Add(";", typeof(SemicolonToken));
            operators.Add(":", typeof(ColonToken));
        }

        internal override bool Recognizes(char character)
        {
            // O(n) in regards of number of operators, but in practise
            // this should not matter
            foreach (var key in operators.Keys)
            {
                if (key.StartsWith("" + character))
                {
                    return true;
                }
            }
            return false;
        }

        protected override Token DoScan()
        {
            // The following is only correct, if all the two character operators
            // start with value that is also a single character operator - otherwise
            // we will get exceptions with malformed operators. Thankfully, this
            // is the case with our current operator set
            string op = "";

            while (Reader.PeekCharacter().HasValue)
            {
                // we get some unnecessary allocations here, but again, 
                // this shouldn't matter
                if (operators.Keys.Contains(op + Reader.PeekCharacter().Value))
                {
                    op += Reader.PeekCharacter().Value;
                }
                else
                {
                    break;
                }
                Reader.NextCharacter();
            }

            return (Token)Activator.CreateInstance(operators[op]);
        }
    }
}
