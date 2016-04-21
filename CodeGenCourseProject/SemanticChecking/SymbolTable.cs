
using System.Collections.Generic;

namespace CodeGenCourseProject.SemanticChecking
{

    internal class SymbolTable
    {
        private int id;

        Stack<SymbolTableLevel> stack;

        public SymbolTable()
        {
            stack = new Stack<SymbolTableLevel>();
            id = 0;
        }

        public void PushLevel()
        {
            stack.Push(new SymbolTableLevel());
        }

        public void PopLevel()
        {
            AssertNotEmpty();

            stack.Pop();
        }

        
        public void InsertSymbol(int line, int column, string name, string type)
        {
            AssertNotEmpty();

            stack.Peek().Add(new Symbol(line, column, name, type, id++));
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

            // FIXME: Stupid way to iterate through the stack
            foreach (var level in stack)
            {
                s = level.GetSymbol(name) ?? s;
            }
            var f = new List<int>();
            
            return s;
        }

        private void AssertNotEmpty()
        {
            if (stack.Count == 0)
            {
                throw new InternalCompilerError("Attempting to operate on symbol table");
            }
        }


        private class SymbolTableLevel
        {
            private IDictionary<string, Symbol> symbols;

            public SymbolTableLevel()
            {
                symbols = new Dictionary<string, Symbol>();
            }

            public void Add(Symbol s)
            {
                if (symbols.ContainsKey(s.Name))
                {
                    throw new InternalCompilerError(
                        "Attempting to insert a variable which is already present on level: " + s.Name);
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
