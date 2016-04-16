using System;

namespace CodeGenCourseProject.Tokens
{
    public abstract class KeywordToken : Token
    {
        protected override Tuple<string, string> GetStringRepresentation()
        {
            return new Tuple<string, string>("keyword", GetKeywordName());
        }

        protected abstract string GetKeywordName();
    }

    public class OrToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "or";
        }
    }

    public class AndToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "and";
        }
    }

    public class NotToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "not";
        }
    }

    public class IfToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "if";
        }
    }

    public class ThenToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "then";
        }
    }

    public class ElseToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "else";
        }
    }

    public class OfToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "of";
        }
    }

    public class WhileToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "while";
        }
    }

    public class DoToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "do";
        }
    }

    public class BeginToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "begin";
        }
    }

    public class EndToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "end";
        }
    }

    public class VarToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "var";
        }
    }

    public class ArrayToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "array";
        }
    }

    public class ProcedureToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "procedure";
        }
    }

    public class FunctionToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "function";
        }
    }

    public class ProgramToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "program";
        }
    }

    public class AssertToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "assert";
        }
    }

    public class ReturnToken : KeywordToken
    {
        protected override string GetKeywordName()
        {
            return "return";
        }
    }
}