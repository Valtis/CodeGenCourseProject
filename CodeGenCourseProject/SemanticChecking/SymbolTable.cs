
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

        public void PushLevel(SymbolTableLevel level)
        {
            stack.Push(level);
        }

        public SymbolTableLevel PopLevel()
        {
            AssertNotEmpty();

            return stack.Pop();
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

        public void InsertFunction(
            int line, int column, string name, string type, IList<string> paramTypes, IList<bool> refParams)
        {
            stack.Peek().Add(
                new FunctionSymbol(line, column, name, type, id++, paramTypes, refParams));
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
    }
}
