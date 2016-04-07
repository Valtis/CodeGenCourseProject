using System;

namespace CodeGenCourseProject.Tokens
{
    public abstract class OperatorToken : Token
    {
        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("operator", GetOperatorName());
        }

        protected abstract string GetOperatorName();
    }

    public class PlusToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "+";
        }
    }

    public class MinusToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "-";
        }
    }

    public class MultiplyToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "*";
        }
    }

    public class DivideToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "/";
        }
    }

    public class ModuloToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "%";
        }
    }

    public class EqualsToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "=";
        }
    }

    public class NotEqualsToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "<>";
        }
    }

    public class LessThanToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "<";
        }
    }

    public class GreaterThanToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return ">";
        }
    }

    public class LessThanOrEqualToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "<=";
        }
    }

    public class GreaterThanOrEqualToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return ">=";
        }
    }

    public class LParenToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "(";
        }
    }

    public class RParenToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return ")";
        }
    }

    public class LBracketToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "[";
        }
    }

    public class RBracketToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return "]";
        }
    }

    public class AssignmentToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return ":=";
        }
    }

    public class PeriodToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return ".";
        }
    }

    public class CommaToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return ",";
        }
    }

    public class SemicolonToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return ";";
        }
    }

    public class ColonToken : OperatorToken
    {
        protected override string GetOperatorName()
        {
            return ":";
        }
    }
}
