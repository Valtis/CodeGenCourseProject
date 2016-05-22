using System.Text;
using System;
using CodeGenCourseProject.Tokens;
using System.Globalization;
using CodeGenCourseProject.ErrorHandling;

namespace CodeGenCourseProject.Lexing
{
    /*
    Scan integers and reals
    */
    internal class NumberScanner : TokenScanner
    {


        internal NumberScanner(TextReader reader, ErrorReporter reporter) : base(reader, reporter)
        {
        }

        internal override bool Recognizes(char character)
        {
            return char.IsDigit(character);
        }

        protected override Token DoScan()
        {
            var line = Reader.Line;
            var column = Reader.Column;
            var builder = new StringBuilder();


            bool sawExponent = false;
            int exponentLine = 0;
            int exponentColumn = 0;

            bool isInteger = true;
            int separatorLine = 0;
            int separatorColumn = 0;

            // effectively a state machine for parsing integers and reals. 
            // Extremely messy, unfortunately
            while (Reader.PeekCharacter().HasValue)
            {
                var character = Reader.PeekCharacter().Value;
                // append any nymbers
                if (char.IsDigit(character))
                {
                    builder.Append(character);
                }
                // negative sign is handled by parser (ex. -93245)
                else if (character == '-' || character == '+')
                {
                    // only part of number if previous char was 'e', otherwise signals end of string
                    var str = builder.ToString();
                    if (str[str.Length - 1] == 'e')
                    {
                        builder.Append(character);
                    }
                    else
                    {
                        break;
                    }
                }
                // if this is first dot, type is converted from integer to real.
                // additional dots are a lexical error
                else if (character == '.')
                {
                    if (isInteger)
                    {
                        isInteger = false;
                        // C# double.Parse expects the decimal separator to be culturally correct
                        builder.Append(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        separatorLine = Reader.Line;
                        separatorColumn = Reader.Column;
                    }
                    else
                    {
                        Reporter.ReportError(
                            Error.LEXICAL_ERROR,
                            "Erroneus extra decimal separator",
                            Reader.Line,
                            Reader.Column);

                        Reporter.ReportError(
                            Error.NOTE,
                            "Original separator is here",
                            separatorLine,
                            separatorColumn);
                    }
                }
                else
                {
                    if (char.IsLetter(character))
                    {
                        if (character == 'e')
                        {
                            // language specification does not allow reals like 123e2
                            if (isInteger)
                            {
                                Reporter.ReportError(
                                    Error.LEXICAL_ERROR,
                                    "Integer cannot contain exponent",
                                    Reader.Line,
                                    Reader.Column);
                            }
                            else
                            {
                                // only first 'e' is acceptable
                                if (!sawExponent)
                                {
                                    sawExponent = true;
                                    builder.Append('e');

                                    exponentLine = Reader.Line;
                                    exponentColumn = Reader.Column;
                                }
                                else
                                {
                                    Reporter.ReportError(
                                        Error.LEXICAL_ERROR,
                                        "Real number cannot contain more than one exponent",
                                        Reader.Line,
                                        Reader.Column);

                                    Reporter.ReportError(
                                      Error.NOTE,
                                      "Original exponent here",
                                      exponentLine,
                                      exponentColumn);
                                }
                            }
                        }
                        else
                        {
                            Reporter.ReportError(
                                Error.LEXICAL_ERROR,
                                "Invalid character '" + character + "' encountered when parsing a number",
                                Reader.Line,
                                Reader.Column);
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                }

                Reader.NextCharacter();
            }


            if (isInteger)
            {
                try
                {
                    return new IntegerToken(int.Parse(builder.ToString()));
                }
                catch (OverflowException e)
                {
                    Reporter.ReportError(
                        Error.LEXICAL_ERROR,
                        "Number does not fit 32 bit signed integer",
                        line,
                        column);

                    return new IntegerToken(1);
                }
            }
            else
            {
                var str = builder.ToString();
                if (str.Contains(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                {
                    var index = str.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    var length = str.Length;

                    // above state machine accepts strings like 123.e3
                    // check for those here
                    if (index == str.Length - 1 || !char.IsDigit(str[index + 1]))
                    {
                        Reporter.ReportError(
                            Error.LEXICAL_ERROR,
                            "Decimal separator must be followed by a number",
                            separatorLine,
                            separatorColumn);
                    }
                }

                // check if ends in 'e'
                if (str[str.Length - 1] == 'e')
                {
                    Reporter.ReportError(
                        Error.LEXICAL_ERROR,
                        "Real cannot end in 'e'",
                        line,
                        column);

                    return new RealToken(1);
                }


                try
                {
                    return new RealToken(double.Parse(builder.ToString()));
                }
                catch (OverflowException e)
                {
                    Reporter.ReportError(
                        Error.LEXICAL_ERROR,
                        "Number does not fit 64 bit real",
                        line,
                        column);

                    return new RealToken(1);
                }
            }

        }
    }
}
