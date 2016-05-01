using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeGenCourseProject.TAC.Values;

namespace CodeGenCourseProject.Codegen
{
    public class CCodeGenerator : CodeGenerator, TACVisitor
    {
        private class Indentation
        {
            private int level;
            private const int SPACES_PER_LEVEL = 4;

            public Indentation()
            {
                level = 0;
            }

            public void Increase()
            {
                level += SPACES_PER_LEVEL;
            }

            public void Decrease()
            {
                level -= SPACES_PER_LEVEL;
            }

            public string Indent
            {
                get
                {
                    return "".PadLeft(level);
                }
            }
        }

        private IList<Function> functions;
        private IList<string> program;
        private Indentation indentation;
        private ISet<string> declared;
        private Stack<string> cValues;

        public CCodeGenerator(IList<Function> functions)
        {
            program = new List<string>();
            this.functions = functions;
            indentation = new Indentation();
            declared = new HashSet<string>();
            cValues = new Stack<string>();
        }

        public void SaveResult(Stream output)
        {
            var encoding = new ASCIIEncoding();
            foreach (var line in program)
            {
                var outLine = line + "\n";
                output.Write(encoding.GetBytes(outLine), 0, encoding.GetByteCount(outLine));
            }
        }

        public void GenerateCode()
        {
            AddHeader();
            foreach (var function in functions)
            {
                GenerateCode(function);
            }
        }

        private void AddHeader()
        {
            EmitInclude("stdio.h");
            EmitInclude("stdlib.h");
            Emit("");
            EmitArrayStruct("int");
        }

        private void EmitArrayStruct(string type)
        {
            string structName = "struct " + type + "_array";
            Emit(structName);
            EmitBlockStart();
            Emit(type + "* arr;");
            Emit("int size;");
            EmitBlockEnd();
            Emit(";");
            Emit("");

            Emit("void __create_" + type + "_array(" + structName + " *in, int size, int line)");
            EmitBlockStart();
            Emit("int is_negative = size < 0;");
            Emit("if (is_negative) goto fail;");
            Emit("size = sizeof(" + type + ") * size;");
            Emit("in->arr = calloc(size);");
            Emit("in->size = size;");
            Emit("return;");
            Emit("fail:");
            Emit("printf(\"Invalid array size at line %d: %d\\n\", line, size);");
            Emit("exit(1);");
            EmitBlockEnd();
            Emit("");

            Emit("void __free_" + type + "_array(" + structName + " *in)");
            EmitBlockStart();
            Emit("free(in->arr);");
            EmitBlockEnd();
            Emit("");

            Emit("void __validate_" + type + "_array_index(" + structName + " *in, int index, int line)");
            EmitBlockStart();
            Emit("int isBelowSize = index < in->size;");
            Emit("int isPositive = index >= 0;");
            Emit("int isValid = isBelowSize && isPositive;");
            Emit("if (isValid) return;");
            Emit("printf(\"Array index out of bounds at line %d: %d\\n\", line, index);");
            Emit("exit(1);");
            EmitBlockEnd();
            Emit("");
        }

        private void GenerateCode(Function function)
        {
            EmitFunctionPrologue(function);
            foreach (var code in function.Statements)
            {
                EmitStatement(code);
            }
            EmitFunctionEpilogue(function);
        }

        private void EmitInclude(string file)
        {
            Emit("#include <" + file + ">");
        }

        private void EmitFunctionPrologue(Function function)
        {
            if (function.Name == TACGenerator.ENTRY_POINT)
            {
                Emit("int main()");
            }

            EmitBlockStart();
        }

        private void EmitStatement(TACStatement statement)
        {

            EmitAssignmentStatement(statement);
        }

        private void EmitAssignmentStatement(TACStatement statement)
        {
            string cStatement = "";
            if (statement.Quad.Item4 != null)
            {
                statement.Quad.Item4.Accept(this);
                cStatement = cValues.Pop() + " = ";
            }

            if (statement.Quad.Item2 != null)
            {
                statement.Quad.Item2.Accept(this);
                cStatement += cValues.Pop();
            }
            if (statement.Quad.Item1.HasValue)
            {
                cStatement += " " + statement.Quad.Item1.Value.Name() + " ";
            }

            if (statement.Quad.Item3 != null)
            {
                statement.Quad.Item3.Accept(this);
                cStatement += cValues.Pop();
            }

            cStatement += ";";
            Emit(cStatement);
        }

        private void EmitFunctionEpilogue(Function function)
        {

            if (function.Name == TACGenerator.ENTRY_POINT)
            {
                Emit("return 0;");
            }
            EmitBlockEnd();
            Emit("");
        }

        private void EmitBlockStart()
        {
            Emit("{");
            indentation.Increase();
        }

        private void EmitBlockEnd()
        {
            indentation.Decrease();
            Emit("}");
        }

        private void Emit(string str)
        {
            program.Add(indentation.Indent + str);
        }

        public void Visit(TACInteger tacInteger)
        {
            cValues.Push(tacInteger.Value.ToString());
        }

        public void Visit(TACIdentifier tacIdentifier)
        {
            var type = "";
            if (!declared.Contains(tacIdentifier.Name))
            {
                declared.Add(tacIdentifier.Name);
                type = GetCType(tacIdentifier.Type) + " ";
            }
            cValues.Push(type + tacIdentifier.Name);
        }

        public void Visit(TACArrayIndex tacArrayIndex)
        {
            tacArrayIndex.Index.Accept(this);
            var index = cValues.Pop();
            var type = GetCType(tacArrayIndex.Type);
            Emit("__validate_" + type + "_array_index(&" + tacArrayIndex.Name + ", " + index + ", __LINE__);");
            cValues.Push(tacArrayIndex.Name + ".arr[" + index + "]");
        }

        public void Visit(TACArrayDeclaration tacArrayDeclaration)
        {
            var type = GetCType(tacArrayDeclaration.Type);
            var name = tacArrayDeclaration.Name;
            var size = tacArrayDeclaration.SizeExpression;
            Emit("struct " + type + "_array " + name + ";");
            cValues.Push("__create_" + type + "_array(&" + tacArrayDeclaration.Name + ", " + size + ", __LINE__)");
        }

        public void Visit(TACCallWriteln tacCallWriteln)
        {
            var formatSpecifiers = new Dictionary<string, string>();
            formatSpecifiers.Add("int", "%d");

            var arguments = tacCallWriteln.Arguments;
            if (arguments.Count == 0)
            {
                cValues.Push("printf(\"\\n\");");
                return;
            }

            var specifierList = new List<string>();
            var argumentList = new List<string>();

            foreach (var arg in arguments)
            {
                arg.Accept(this);
                argumentList.Add(cValues.Pop());
                specifierList.Add(formatSpecifiers[GetCType(arg)]);
            }

            cValues.Push("printf(\"" + string.Join(", ", specifierList) + "\\n\", " + string.Join(", ", argumentList) + ");");

        }
        
        private string GetCType(string miniPLType)
        {
            string cType;
            switch (miniPLType)
            {
                case SemanticChecker.INTEGER_TYPE:
                    return "int";
                case SemanticChecker.BOOLEAN_TYPE:
                    return "char";                    
                default:
                    throw new InternalCompilerError("Should not be reached");
            }
        }

        private string GetCType(TACValue v)
        {
            if (v is TACInteger)
            {
                return "int";
            }

            if (v is TACIdentifier)
            {
                return GetCType(((TACIdentifier)v).Type);
            }

            if (v is TACArrayIndex)
            {
                return GetCType(((TACArrayIndex)v).Type);
            }

            throw new NotImplementedException();

        }

        public void Visit(TACReal tacReal)
        {
            throw new NotImplementedException();
        }

        public void Visit(TACBoolean tacBoolean)
        {
            cValues.Push((tacBoolean.Value ? 1 : 0).ToString());
        }

        public void Visit(TACString tacString)
        {
            throw new NotImplementedException();
        }

        public void Visit(TACArraySize tacArraySize)
        {
            throw new NotImplementedException();
        }

        public void Visit(TACLabel tacLabel)
        {
            cValues.Push("label_" + tacLabel.ID + ":");
        }

        public void Visit(TACJumpIfFalse tacJumpIfTrue)
        {
            tacJumpIfTrue.Condition.Accept(this);
            Emit("if (!" + cValues.Pop()  + ")");
            cValues.Push("goto label_" + tacJumpIfTrue.Label.ID);
        }

        public void Visit(TACJump tacJump)
        {
            cValues.Push("goto label_" + tacJump.Label.ID);
        }
    }
}
