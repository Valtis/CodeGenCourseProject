using System;
using System.Collections.Generic;
using System.Text;
using CodeGenCourseProject.Tokens;
using CodeGenCourseProject.ErrorHandling;

namespace CodeGenCourseProject.Lexing
{

    /*
    Scans identifers and keywords. Uses screening for keywords
    */
    internal class IdentifierAndKeywordScanner : TokenScanner
    {

        private IDictionary<string, Type> keywords;

        internal IdentifierAndKeywordScanner(TextReader reader, ErrorReporter reporter) : base(reader, reporter)
        {
            keywords = new Dictionary<string, Type>();

            keywords.Add("or", typeof(OrToken));
            keywords.Add("and", typeof(AndToken));
            keywords.Add("not", typeof(NotToken));
            keywords.Add("if", typeof(IfToken));
            keywords.Add("then", typeof(ThenToken));
            keywords.Add("else", typeof(ElseToken));
            keywords.Add("of", typeof(OfToken));
            keywords.Add("while", typeof(WhileToken));
            keywords.Add("do", typeof(DoToken));
            keywords.Add("begin", typeof(BeginToken));
            keywords.Add("end", typeof(EndToken));
            keywords.Add("var", typeof(VarToken));
            keywords.Add("array", typeof(ArrayToken));
            keywords.Add("procedure", typeof(ProcedureToken));
            keywords.Add("function", typeof(FunctionToken));
            keywords.Add("program", typeof(ProgramToken));
            keywords.Add("assert", typeof(AssertToken));
        }

        internal override bool Recognizes(char character)
        {
            return char.IsLetter(character);
        }

        protected override Token DoScan()
        {
             var builder = new StringBuilder();

             while (Reader.PeekCharacter().HasValue && 
                 (char.IsLetterOrDigit(Reader.PeekCharacter().Value) || 
                 Reader.PeekCharacter().Value == '_'))
             {
                 builder.Append(Reader.PeekCharacter().Value);
                 Reader.NextCharacter();
             }

             var text = builder.ToString();
             if (keywords.ContainsKey(text))
             {
                 return (Token)Activator.CreateInstance(keywords[text]);
             }

             return new IdentifierToken(text);
        }


    }
}
