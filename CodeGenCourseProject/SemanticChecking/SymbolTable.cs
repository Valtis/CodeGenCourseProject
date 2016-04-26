
using CodeGenCourseProject.ErrorHandling;
using System.Collections.Generic;

namespace CodeGenCourseProject.SemanticChecking
{

    internal class SymbolTable
    {
        private int id;
        private ErrorReporter reporter;
        Stack<SymbolTableLevel> stack;

        public SymbolTable(ErrorReporter reporter)
        {
            this.reporter = reporter;
            stack = new Stack<SymbolTableLevel>();
            id = 0;
        }

        public void PushLevel()
        {
            stack.Push(new SymbolTableLevel(reporter));
        }

        public void PopLevel()
        {
            AssertNotEmpty();

            stack.Pop();
        }

        
        public void InsertVariable(int line, int column, string name, string type)
        {
            AssertNotEmpty();

            stack.Peek().Add(new VariableSymbol(line, column, name, type, id++));
        }

        public void InsertArray(int line, int column, string name, string type)
        {
            AssertNotEmpty();

            stack.Peek().Add(new ArraySymbol(line, column, name, type, id++));
        }

        public void InsertFunction(int line, int column, string name, string type, IList<string> paramTypes)
        {
            stack.Peek().Add(new FunctionSymbol(line, column, name, type, id++, paramTypes));
        }

        public bool ContainsOnLevel(string name)
        {
            AssertNotEmpty();
            return stack.Peek().Contains(name);
        }

        public bool Contains(string name)
        {
            AssertNotEmpty();
            foreach (var level in stack)
            {
                if (level.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        public Symbol GetSymbol(string name)
        {
            AssertNotEmpty();
            Symbol s = null;
            
            foreach (var level in stack)
            {
                s = level.GetSymbol(name);
                if (s != null)
                {
                    return s;
                }
            }            
            return s;
        }

        private void AssertNotEmpty()
        {
            if (stack.Count == 0)
            {
                throw new InternalCompilerError("Attempting to operate on empty symbol table");
            }
        }


        private class SymbolTableLevel
        {
            private IDictionary<string, Symbol> symbols;
            private ErrorReporter reporter;
            public SymbolTableLevel(ErrorReporter reporter)
            {
                this.reporter = reporter;
                symbols = new Dictionary<string, Symbol>();
            }

            public void Add(Symbol s)
            {
                if (symbols.ContainsKey(s.Name))
                {
                    reporter.ReportError(
                        Error.SEMANTIC_ERROR,
                        "Redeclaration of identifier '" + s.Name + "'",
                        s.Line,
                        s.Column);

                    var original = GetSymbol(s.Name);

                    reporter.ReportError(
                        Error.NOTE,
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
    }
}
