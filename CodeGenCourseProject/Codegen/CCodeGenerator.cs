using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CodeGenCourseProject.TAC.Values;
using System.Linq;
using System.Globalization;
using static CodeGenCourseProject.TAC.Function;
using System.Threading;

namespace CodeGenCourseProject.Codegen
{
    public class CCodeGenerator : CodeGenerator, TACVisitor
    {
        private const string C_INTEGER = "int";
        private const string C_REAL = "double";
        private const string C_BOOLEAN = "char";
        private const string C_STRING = "string";
        private const string C_VOID = "void";

        private const string C_INTEGER_ARRAY = C_INTEGER + "_array";
        private const string C_REAL_ARRAY = C_REAL + "_array";
        private const string C_BOOLEAN_ARRAY = C_BOOLEAN + "_array";
        private const string C_STRING_ARRAY = C_STRING + "_array";

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
            // ensure decimal values use '.' instead of ',' as decimal separator
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
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
            Emit("/***** START OF AUTO-GENERATED HELPER CODE *****/");
            Emit("");
            Emit("typedef const char * string;");
            Emit("");
            EmitInclude("stdio.h");
            EmitInclude("stdlib.h");
            Emit("");
            EmitArrayStruct(C_INTEGER);
            EmitArrayStruct(C_REAL);
            EmitArrayStruct(C_BOOLEAN);
            EmitArrayStruct(C_STRING);
            EmitStringFunctions();
            Emit("");
            Emit("/***** END OF AUTO-GENERATED HELPER CODE *****/");
            Emit("");
        }
        
        private void EmitArrayStruct(string type)
        {
            string structName = type + "_array";
            Emit(@"
typedef struct 
{
    " + type + @"* arr;
    int size;
} " + structName + @";
");

            Emit(@"
void __create_" + type + @"_array(" + structName + @" *in, int size, int line)
{
    int is_negative = size < 0;
    if (is_negative) goto fail;
    size_t elem_size = sizeof(" + type + @");
    in->arr = calloc(size, elem_size);
    in->size = size;
");
            // If this is string array, initialize the array with empty strings rather than 
            // null pointers. Otherwise operations like foo[0] + foo[1] would segfault
            if (type == C_STRING)
            {
                Emit(@"
    int i;
    for (i = 0; i < size; ++i)
    {
        in->arr[i] = """";        
    }");
            }
            Emit(@"
    return;
    fail:
    printf(""Invalid array size at line % d: % d\n"", line, size);
    exit(1);
}
");

            Emit("void __validate_" + type + "_array_index(" + structName + " *in, int index, int line)");
            EmitBlockStart();
            Emit("int isBelowSize = index < in->size;");
            Emit("int isPositive = index >= 0;");
            Emit("int isValid = isBelowSize && isPositive;");
            Emit("if (isValid) return;");
            Emit("printf(\"Array index out of bounds at line %d: Was %d when array has size %d\\n\", line, index, in->size);");
            Emit("exit(1);");
            EmitBlockEnd();
            Emit("");
        }

        private void EmitStringFunctions()
        {
            Emit(@"
int str_len(const char *str)
{
    int length = 0;
    while (*str != '\0')
    {
        length++;
        str++;
    }
    return length;
}

const char *str_concat(const char *lhs, const char *rhs)
{
    int lhs_size = str_len(lhs);
    int rhs_size = str_len(rhs); 
    int size = lhs_size + rhs_size;
    size++; // null terminator
    char *dst = malloc(size);
 
    while (*lhs != '\0')
    {
        *dst++ = *lhs++; 
    }
    
    while (*rhs != '\0')
    {
        *dst++ = *rhs++; 
    }

    int null_pos = size-1;
    dst[null_pos] = '\0';
    // rewind pointer back to start
    dst = dst - null_pos;
    
    return dst;                
}");
        }

        private void GenerateCode(Function function)
        {
            declared.Clear();
            DeclareParameters(function);
            EmitFunctionPrologue(function);
            foreach (var code in function.Statements)
            {
                EmitStatement(code);
            }
            EmitFunctionEpilogue(function);
        }

        private void DeclareParameters(Function function)
        {
            foreach (var param in function.Parameters)
            {
                declared.Add(param.Identifier.Name);
            }
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
            else
            {
                var param_list = function.Parameters.Select(x => 
                GetCType(x.Type) + " " +( x.IsReference ? "*" : "") + x.Identifier.Name);
                var param = string.Join(", ", param_list);
                Emit(GetCType(function.ReturnType) + " " + function.Name + "(" + param + ")");
            }

            EmitBlockStart();
        }

        private void EmitStatement(TACStatement statement)
        {
            
            string dest = "" ;
            string lhs = "";
            string rhs = "";
            string operation = "";
            string cStatement = "";

            /*
            Reference handling is here and not in, say, Visit(TACIdentifier), as
            whether or not we want to insert *, & or nothing depends on the context where it is used
            and Visit(TACIdentifier) does not have enough contextual information to decide this.

            E.g. a is reference, we want
            *a := 4;
            
            but foo(a); 
           
            if a expects reference parameter


            */
            if (statement.Destination != null)
            {
                statement.Destination.Accept(this);

                var isReference = false;
                if (statement.Destination is TACIdentifier && ((TACIdentifier)statement.Destination).IsReference)
                {
                    isReference = true;
                }

                if (isReference)
                {
                    // dereference
                    dest += "*";
                }
                dest += cValues.Pop() + " = ";
            }

            if (statement.LeftOperand != null)
            {
                statement.LeftOperand.Accept(this);

                var isReference = false;
                if (statement.LeftOperand is TACIdentifier && ((TACIdentifier)statement.LeftOperand).IsReference)
                {
                    isReference = true;
                }

                if (isReference)
                {
                    // dereference
                    lhs += "*";
                }
                lhs += cValues.Pop();
            }

            if (statement.RightOperand != null)
            {
                statement.RightOperand.Accept(this);

                var isReference = false;
                if (statement.RightOperand is TACIdentifier && ((TACIdentifier)statement.RightOperand).IsReference)
                {
                    isReference = true;
                }

                if (isReference)
                {
                    // dereference
                    rhs += "*";
                }

                rhs += cValues.Pop();
            }
                // rhs should never be empty if we have an operator
            operation = HandleOperator(lhs, rhs, statement.Operator, GetCType(statement.RightOperand));


            cStatement = dest + operation +  ";";
            Emit(cStatement);
        }



        string HandleOperator(string lhs, string rhs, Operator? op, string type)
        {
            if(op.HasValue)
            {
                if (op == Operator.CONCAT)
                {
                    return "str_concat(" + lhs + ", " + rhs + ")";
                }
                else if (type == SemanticChecker.STRING_TYPE)
                {
                    return "strcmp(" + lhs + ", " + rhs + ") " + op.Value.Name() + " 0";
                }
                else
                {
                    return lhs + " " + op.Value.Name() + " " + rhs;
                }
            }

            return lhs + " " + rhs;
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
            
            // arrays are alwayws pre-declared, non-arrays are declared on demand
            if (!declared.Contains(tacIdentifier.Name) && !tacIdentifier.Type.Contains(SemanticChecker.ARRAY_PREFIX))
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

            var addressOperator = "";
            var memberOperator = ".";
            if (tacArrayIndex.IsReference)
            {
                memberOperator = "->";
            }
            else
            {
                addressOperator = "&";
            }

            Emit("__validate_" + type + "_array_index(" + addressOperator + "" + tacArrayIndex.Name + ", " + index + "," + (tacArrayIndex.Line + 1) + " );");
            cValues.Push(tacArrayIndex.Name + memberOperator + "arr[" + index + "]");
        }

        public void Visit(TACArrayDeclaration tacArrayDeclaration)
        {
            var type = GetCType(tacArrayDeclaration.Type);
            var name = tacArrayDeclaration.Name;
            var size = tacArrayDeclaration.Expression;
            Emit(type + "_array " + name + ";");
            cValues.Push("__create_" + type + "_array(&" + tacArrayDeclaration.Name + ", " + size + "," + (tacArrayDeclaration.Line + 1) + ")");
        }

        public void Visit(TACCallWriteln tacCallWriteln)
        {
            var formatSpecifiers = new Dictionary<string, string>();
            formatSpecifiers.Add(C_INTEGER, "%d");
            formatSpecifiers.Add(C_REAL, "%f");
            formatSpecifiers.Add(C_BOOLEAN, "%d"); // treat as integer
            formatSpecifiers.Add(C_STRING, "%s");

            var arguments = tacCallWriteln.Arguments;
            if (arguments.Count == 0)
            {
                cValues.Push("printf(\"\\n\")");
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

            cValues.Push("printf(\"" + string.Join("", specifierList) + "\\n\", " + string.Join(", ", argumentList) + ")");

        }
        
        private string GetCType(string miniPLType)
        {
            switch (miniPLType)
            {
                case SemanticChecker.INTEGER_TYPE:
                    return C_INTEGER;
                case SemanticChecker.REAL_TYPE:
                    return C_REAL;
                case SemanticChecker.BOOLEAN_TYPE:
                    return C_BOOLEAN;
                case SemanticChecker.STRING_TYPE:
                    return C_STRING;
                case SemanticChecker.VOID_TYPE:
                    return C_VOID;
                case SemanticChecker.INTEGER_ARRAY:
                    return C_INTEGER_ARRAY;
                case SemanticChecker.REAL_ARRAY:
                    return C_REAL_ARRAY;
                case SemanticChecker.STRING_ARRAY:
                    return C_STRING_ARRAY;
                case SemanticChecker.BOOLEAN_ARRAY:
                    return C_BOOLEAN_ARRAY;

                default:
                    throw new InternalCompilerError("Should not be reached");
            }
        }

        private string GetCType(TACValue v)
        {
            if (v is TACInteger)
            {
                return C_INTEGER;
            }

            if (v is TACReal)
            {
                return C_REAL;
            }

            if (v is TACBoolean)
            {
                return C_BOOLEAN;
            }

            if (v is TACString)
            {
                return C_STRING;
            }
            
            if (v is TACIdentifier)
            {
                return GetCType(((TACIdentifier)v).Type);
            }

            if (v is TACArrayIndex)
            {
                return GetCType(((TACArrayIndex)v).Type);
            }

            return C_VOID;
        }

        public void Visit(TACReal tacReal)
        {
            cValues.Push(tacReal.Value.ToString());
        }

        public void Visit(TACBoolean tacBoolean)
        {
            cValues.Push((tacBoolean.Value ? 1 : 0).ToString());
        }

        public void Visit(TACString tacString)
        {
            cValues.Push("\"" + tacString.Value + "\"");
        }

        public void Visit(TACArraySize tacArraySize)
        {
            throw new NotImplementedException();
        }

        public void Visit(TACLabel tacLabel)
        {
            cValues.Push("____label_" + tacLabel.ID + ":");
        }

        public void Visit(TACJumpIfFalse tacJumpIfTrue)
        {
            tacJumpIfTrue.Condition.Accept(this);
            Emit("if (!" + cValues.Pop()  + ")");
            cValues.Push("goto ____label_" + tacJumpIfTrue.Label.ID);
        }

        public void Visit(TACJump tacJump)
        {
            cValues.Push("goto ____label_" + tacJump.Label.ID);
        }

        public void Visit(TACCall tacCall)
        {
            if (tacCall is TACCallRead)
            {
                Visit((TACCallRead)tacCall);
                return;
            }

            if (tacCall is TACCallWriteln)
            {
                Visit((TACCallWriteln)tacCall);
                return;
            }
            
            IList<Parameter> parameters = null;
            foreach (var function in functions)
            {
                if (function.Name == tacCall.Function)
                {
                    parameters = function.Parameters;
                    break;
                }
            }
            var args_string = new List<string>();

            for (int i = 0; i < tacCall.Arguments.Count; ++i)
            {
                string arg = "";
                var argIsRef = parameters[i].IsReference;
                var paramIsRef = false;
                if (tacCall.Arguments[i] is TACIdentifier)
                {
                    var identifier = (TACIdentifier)tacCall.Arguments[i];
                    paramIsRef = identifier.IsReference;
                }
                if (argIsRef && !paramIsRef)
                {
                    arg += "&";
                }
                else if (!argIsRef && paramIsRef)
                {
                    arg += "*";
                }

                tacCall.Arguments[i].Accept(this);
                arg += cValues.Pop();
                args_string.Add(arg);
            };
            var args = string.Join(", ", args_string);

            cValues.Push(tacCall.Function + "(" + args + ")");
        }

        public void Visit(TACReturn tacReturn)
        {
            var expr = "";
            if (tacReturn.Expression != null)
            {
                tacReturn.Expression.Accept(this);
                expr = cValues.Pop();
            }
            cValues.Push("return " + expr);
        }

        public void Visit(TACAssert tacAssert)
        {
            throw new NotImplementedException();
        }
    }
}
