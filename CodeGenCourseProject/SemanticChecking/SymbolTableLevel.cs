using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.SemanticChecking;
using System.Collections.Generic;

internal class SymbolTableLevel
{
    private IDictionary<string, Symbol> symbols;
    private MessageReporter reporter;
    public SymbolTableLevel(MessageReporter reporter)
    {
        this.reporter = reporter;
        symbols = new Dictionary<string, Symbol>();
    }

    public void Add(Symbol s)
    {
        if (symbols.ContainsKey(s.Name))
        {
            reporter.ReportError(
                MessageKind.SEMANTIC_ERROR,
                "Redeclaration of identifier '" + s.Name + "'",
                s.Line,
                s.Column);

            var original = GetSymbol(s.Name);

            reporter.ReportError(
                MessageKind.NOTE,
                "Identifier '" + original.Name + "' was declared here",
                original.Line,
                original.Column);
            return;
        }
        symbols.Add(s.Name, s);
    }

    public bool Contains(string name)
    {
        return symbols.ContainsKey(name);
    }

    public Symbol GetSymbol(string name)
    {
        if (symbols.ContainsKey(name))
        {
            return symbols[name];
        }

        return null;
    }

}