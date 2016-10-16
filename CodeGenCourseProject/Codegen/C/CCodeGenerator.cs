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

namespace CodeGenCourseProject.Codegen.C
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

        private const string C_LABEL_PREFIX = "____label_";

        private const string CAPTURE_NOTIFIER = "CAPTURED";

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
        private ISet<Variable> capturedVariables;
        private ISet<string> declared;
        private Stack<string> cValues;
        private Stack<TACValue> argStack;

        public CCodeGenerator(IList<Function> functions)
        {
            // ensure decimal values use '.' instead of ',' as decimal separator
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            program = new List<string>();
            this.functions = functions;
            indentation = new Indentation();
            declared = new HashSet<string>();
            cValues = new Stack<string>();
            argStack = new Stack<TACValue>();
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
            Emit("#define " + CAPTURE_NOTIFIER);
            Emit("");
            EmitInclude("stdio.h");
            EmitInclude("stdlib.h");
            EmitInclude("string.h");
            EmitInclude("stdarg.h");
            Emit("");
            EmitGC();
            EmitArrayStruct(C_INTEGER);
            EmitArrayStruct(C_REAL);
            EmitArrayStruct(C_BOOLEAN);
            EmitArrayStruct(C_STRING, GC.C_OBJ_TYPE_STRING_ARRAY);
            EmitStringFunctions();
            EmitRead();
            EmitAssert();
            Emit("");
            Emit("/***** END OF AUTO-GENERATED HELPER CODE *****/");
            Emit("");
        }

        private void EmitGC()
        {
            Emit(GC.GetGCCode());
        }

        private void EmitArrayStruct(string type, string gc_type = GC.C_OBJ_TYPE_NONE)
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
    if (size < 0)
    {
        printf(""Invalid array size at line %d: %d\n"", line, size);
        exit(1);
    }
    
    size_t elem_size = sizeof(" + type + @");
    in->arr = gc_calloc(size, elem_size, " + gc_type + @");
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

            Emit(@"void __copy_" + type + "_array(const " + structName + " * const src, " + structName + @" *dst)
{
    dst->size = src->size;
    dst->arr = gc_malloc(src->size * sizeof(" + type + @"), " + gc_type + @");
    int i = 0;
    for (; i < src->size; ++i)
    {
        dst->arr[i] = src->arr[i];
    }
}");
        }

        private void EmitStringFunctions()
        {
            Emit(@"
const char *str_concat(const char *lhs, const char *rhs)
{
    int lhs_size = strlen(lhs);
    int rhs_size = strlen(rhs); 
    int size = lhs_size + rhs_size;
    size++; // null terminator
    char *dst = gc_malloc(size, " + GC.C_OBJ_TYPE_NONE + @");
 
    while (*lhs != '\0')
    {
        *dst++ = *lhs++; 
    }
    
    while (*rhs != '\0')
    {
        *dst++ = *rhs++; 
    }

    // rewind pointer back to start
    int null_pos = size-1;
    dst = dst - null_pos;
    dst[null_pos] = '\0';
    
    return dst;                
}");
        }

        private void EmitRead()
        {
            Emit(@"

#ifndef READ_BUFFER_SIZE
#define READ_BUFFER_SIZE 256
#endif
/*
Read up to READ_BUFFER_SIZE-1 characters from the standard input. String is tokenized using single space.
Tokens are converted to desired types. If there are less tokens than desired, rest of the values are
default initialized (0, 0.0 for integer/real, """" for string)
*/
void read(const char *format, ...)
{
    // dynamic allocation in case the buffer size is defined to be really big
    // and it ends up causing stack overflow if dynamic allocation is not used
    char *buffer = gc_malloc(READ_BUFFER_SIZE, " + GC.C_OBJ_TYPE_NONE + @");
    fgets(buffer, READ_BUFFER_SIZE, stdin);
	va_list args;
    va_start(args, format);
    char *token = strtok(buffer, "" "");

    while (*format != '\0')
    {
        if (*format == 'd')
        {
            int *ptr = va_arg(args, int *);
            if (token == NULL)
            {
                *ptr = 0;
            }
            else 
            {
                *ptr = atoi(token);
            }            
        }
        else if (*format == 'f')
        {
            double *ptr = va_arg(args, double *);
            if (token == NULL)
            {
                *ptr = 0;
            }
            else 
            {
                *ptr = atof(token);
            }       
        }
        else if (*format == 's')
        {
            string *ptr = va_arg(args , string *);
            if (token == NULL)
            {
                *ptr = """";
            }
            else 
            {
                /*
                    A copy is required due to how the GC works: It only considers the start of the pointer when
                    determining if object is alive or not. If we store the token directly, we are storing
                    a pointer that likely points to the middle of the gc_malloc'd pointer, which is then
                    considered to be free. This means we would be facing use-after-free bugs in the program.
                    While changing the GC to consider the whole range (start -> start + alloc_size) wouldn't
                    be difficult, I still opt for the lazier approach here, which is to copy the string
                    to new gc_malloc'd ptr.
                */
                size_t size = strlen(token) + 1;
                char *new_str = gc_malloc(size, " + GC.C_OBJ_TYPE_NONE + @");
                strcpy(new_str, token);
                new_str[size-1] = '\0';
                if (new_str[size-2] == '\n')
                    new_str[size-2] = '\0'; // erase newline in case this was the last arg                
                *ptr = new_str;
            }       
        }

        token = strtok(NULL, "" "");
        ++format;
    }
    va_end(args);
}");
        }
        private void EmitAssert()
        {
            Emit(@"
void assert(char expr, int line)
{
    if (!expr)
    {
        printf(""Assert failed at line %d\n"", line);
        exit(1);
    }
}");
        }

        private void GenerateCode(Function function)
        {
            declared.Clear();
            declared.UnionWith(function.CapturedVariables.Select(x => x.Identifier.Name));
            capturedVariables = function.CapturedVariables;

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
            if (function.Name == Generator.ENTRY_POINT)
            {
                Emit("int main()");
            }
            else
            {
                var param_list = new List<string>(function.Parameters.Select(x =>
                GetCType(x.Type) + " " + (x.IsReference ? "*" : "") + x.Identifier.Name));

                param_list.AddRange(
                    function.CapturedVariables.Select(x => CAPTURE_NOTIFIER + " " + GetCType(x.Type) + " *" + x.Identifier.Name));

                var param = string.Join(", ", param_list);
                Emit(GetCType(function.ReturnType) + " " + function.Name + "(" + param + ")");
            }

            EmitBlockStart();

            if (function.Name == Generator.ENTRY_POINT)
            {
                Emit("/*** AUTO-GENERATED CODE ***/");
                Emit("#ifndef GC_DISABLE");
                Emit("gc_init(__builtin_frame_address(0));");
                Emit("#endif");
                Emit("/*** END OF AUTO-GENERATED CODE ***/");
            }
        }

        private void EmitStatement(Statement statement)
        {
            /* REFACTOR REFACTOR REFACTOR */
           
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

            if (statement.Operator.HasValue)
            {
                switch (statement.Operator)
                {
                    case Operator.PUSH:
                    case Operator.PUSH_INITIALIZED:
                        // PUSH_INITIALIZED may push variables that have not been used elsewhere yet, 
                        // so ensure they are declared
                        if (statement.RightOperand is TACIdentifier)
                        {
                            var type = GetTypeIfNotDeclared((TACIdentifier)statement.RightOperand);
                            if (type != String.Empty)
                            {
                                Emit(type + ((TACIdentifier)statement.RightOperand).Name + ";");
                            }
                        }

                        argStack.Push(statement.RightOperand);
                        return;
                    case Operator.CALL_WRITELN:
                        EmitWriteLn(((TACInteger)statement.RightOperand).Value);
                        return;
                    case Operator.CALL_READ:
                        EmitRead(((TACInteger)statement.RightOperand).Value);
                        return;
                    case Operator.CALL_ASSERT:
                        EmitAssert(statement);
                        return;
                    case Operator.CALL:
                        EmitFunctionCall(statement);
                        return;
                    case Operator.LABEL:
                        EmitLabel(statement);
                        return;
                    case Operator.JUMP:
                        EmitJump(statement);
                        return;
                    case Operator.JUMP_IF_FALSE:
                        EmitConditionalJump(statement);
                        return;
                    case Operator.RETURN:
                        EmitReturn(statement);
                        return;
                    case Operator.VALIDATE_INDEX:
                        EmitValidateArrayIndex(statement);
                        return;
                    case Operator.ARRAY_SIZE:
                        EmitArraySize(statement);
                        return;
                    case Operator.CLONE_ARRAY:
                        EmitArrayClone(statement);
                        return;
                    case Operator.DECLARE_ARRAY:
                        EmitArrayDeclaration(statement);
                        return;
                    default:
                        break;

                }
            }

            string dest = GenerateDestination(statement);

            if (statement.LeftOperand != null)
            {
                statement.LeftOperand.Accept(this);
                lhs += GetDereferenceOperator(statement.LeftOperand) + cValues.Pop();
            }

            if (statement.RightOperand != null)
            {
                statement.RightOperand.Accept(this);
                rhs += GetDereferenceOperator(statement.RightOperand) + cValues.Pop();
            }
            // rhs should never be empty if we have an operator
            operation = HandleOperator(lhs, rhs, statement.Operator, GetCType(statement.RightOperand));


            cStatement = dest + operation + ";";
            Emit(cStatement);
        }

        private string GenerateDestination(Statement statement)
        {
            string dest = string.Empty;
            if (statement.Destination != null)
            {

                statement.Destination.Accept(this);
                string type = String.Empty;
                if (statement.Destination is TACIdentifier)
                {
                    type = GetTypeIfNotDeclared((TACIdentifier)statement.Destination);
                }

                if (type != String.Empty)
                {
                    dest += type;
                }
                else
                {
                    dest += GetDereferenceOperator(statement.Destination);
                }

                dest += cValues.Pop() + " = ";
            }

            return dest;
        }

        string HandleOperator(string lhs, string rhs, Operator? op, string type)
        {
            if (op.HasValue)
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

            if (function.Name == Generator.ENTRY_POINT)
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

            // arrays are alwayws pre-declared, non-arrays are declared on demand
            if (tacIdentifier.IsArray)
            {
                cValues.Push(tacIdentifier.Name + (tacIdentifier.IsReference || capturedVariables.Any(x => x.Identifier.Name == tacIdentifier.Name) ? "->" : ".") + "arr");
            } 
            else
            {
                cValues.Push(tacIdentifier.Name);
            }
        }

        private string GetTypeIfNotDeclared(TACIdentifier tacIdentifier)
        {
            var type = "";
            if (!declared.Contains(tacIdentifier.Name) &&
                // non-temporary arrays are already pre-declared due to them requiring array size expression.
                // Temporary arrays are always assigned into (function return value), and can be declared on-demand 
                (!tacIdentifier.Type.Contains(SemanticChecker.ARRAY_PREFIX) || tacIdentifier.Name.StartsWith("__t")))
            {
                declared.Add(tacIdentifier.Name);
                type = GetCType(tacIdentifier.Type) + " ";
                if (tacIdentifier.IsReference)
                {
                    type += "*";
                }
            }

            return type;
        }

        public void EmitArrayDeclaration(Statement statement)
        {
            var array = (TACIdentifier)statement.LeftOperand;
            statement.RightOperand.Accept(this);

            var type = GetCType(array.Type);
            var name = array.Name;
            var sizeExpr = GetDereferenceOperator(statement.LeftOperand) + cValues.Pop();

            Emit(ArrayCreation(name, type, sizeExpr, (array.Line + 1).ToString()) + ";");
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
            // we need to convert \n back to the escape sequence form, as line break in C string is not 
            // acceptable without appending \ to the line. Similarily, \" must be replaced by the escape sequence, 
            // as otherwise the string is broken by unexpected '"' characters

            // we also need to change \ to \\, as otherwise it is interpreted as start of escape sequence by the C compiler

            string value = tacString.Value;
            // ordering is critical here, as otherwise the escape secuence character gets malformed
            value = value.Replace("\\", "\\\\");
            value = value.Replace("\n", "\\n");
            value = value.Replace("\"", "\\\"");
            value = value.Replace("\r", "\\r"); // carriage return seems to be interpreted as new line as well

            cValues.Push("\"" + value + "\"");
        }

        public void EmitArraySize(Statement statement)
        {
            statement.RightOperand.Accept(this);
            var memberOp = ".";
            if (statement.RightOperand is TACIdentifier)
            {
                var ident = (TACIdentifier)statement.RightOperand;
                if (ident.IsReference ||
                    capturedVariables.Any(x => x.Identifier.Name == ident.Name))
                {
                    memberOp = "->";
                }
            }
            var destination = GenerateDestination(statement);
            Emit(destination + cValues.Pop() + memberOp + "size;");

        }

        public void EmitAssert(Statement statement)
        {
            statement.RightOperand.Accept(this);
            var line = (TACInteger)statement.LeftOperand;
            Emit("assert(" + cValues.Pop() + ", " + (line.Value + 1) + ");");
        }

        public void EmitWriteLn(int argCount)
        {
            var formatSpecifiers = new Dictionary<string, string>();
            formatSpecifiers.Add(C_INTEGER, "%d");
            formatSpecifiers.Add(C_REAL, "%f");
            formatSpecifiers.Add(C_BOOLEAN, "%d"); // treat as integer
            formatSpecifiers.Add(C_STRING, "%s");

            if (argCount == 0)
            {
                Emit("printf(\"\\n\");");
                return;
            }

            var tacValueArguments = new List<TACValue>();

            for (int i = 0; i < argCount; ++i)
            {
                tacValueArguments.Add(argStack.Pop());
            }

            var specifierList = new List<string>();
            var argumentList = new List<string>();

            foreach (var arg in tacValueArguments)
            {
                arg.Accept(this);
                var prefix = GetDereferenceOperator(arg);
                argumentList.Add(prefix + cValues.Pop());
                specifierList.Add(formatSpecifiers[GetCType(arg)]);
            }

            Emit("printf(\"" + string.Join("", specifierList) + "\\n\", " + string.Join(", ", argumentList) + ");");
        }

        void EmitLabel(Statement statement)
        {
            int id = ((TACInteger)statement.RightOperand).Value;
            Emit(getLabel(id) + ":;");
        }

        void EmitJump(Statement statement)
        {
            int id = ((TACInteger)statement.RightOperand).Value;
            Emit("goto " + getLabel(id) + ";");
        }

        void EmitConditionalJump(Statement statement)
        {
            statement.LeftOperand.Accept(this);
            int id = ((TACInteger)statement.RightOperand).Value;
            var derefOp = GetDereferenceOperator(statement.LeftOperand);

            Emit("if (!" + derefOp + cValues.Pop() + ")");
            indentation.Increase();
            Emit("goto " + getLabel(id) + ";");
            indentation.Decrease();
        }

        public void EmitReturn(Statement statement)
        {
            var expr = "";
            if (statement.RightOperand != null)
            {
                statement.RightOperand.Accept(this);
                expr = cValues.Pop();
            }

            Emit("return " + GetDereferenceOperator(statement.RightOperand) + expr + ";");
        }

        public void EmitRead(int argCount)
        {
            var argStrings = new List<string>();
            string formatString = "";

            var tacValueArguments = new List<TACValue>();

            for (int i = 0; i < argCount; ++i)
            {
                tacValueArguments.Add(argStack.Pop());
            }

            foreach (var arg in tacValueArguments)
            {
                arg.Accept(this);
                var addressSymbol = "&";
                var type = "";
                if (arg is TACIdentifier)
                {
                    var ident = (TACIdentifier)arg;
                    type = ident.Type;
                    if (ident.IsReference || capturedVariables.Any(x => x.Identifier.Name == ident.Name))
                    {
                        addressSymbol = "";
                    }
                }

                switch (type)
                {
                    case SemanticChecker.INTEGER_TYPE:
                        formatString += "d";
                        break;
                    case SemanticChecker.REAL_TYPE:
                        formatString += "f";
                        break;
                    case SemanticChecker.STRING_TYPE:
                        formatString += "s";
                        break;
                    default:
                        throw new InternalCompilerError("Invalid type " + type + " when handling types for 'read'");
                }

                string value = cValues.Pop();
                // count > 1 ---> it was declared only now (first use)
                if (value.Split().Length > 1)
                {
                    Emit(value + ";");
                    argStrings.Add(addressSymbol + value.Split()[1]);
                }
                else
                {
                    argStrings.Add(addressSymbol + value);
                }
            }
            
            var args = string.Join(", ", argStrings);
            Emit("read(\"" + formatString + "\", " + args + ");");
        }


        public void EmitFunctionCall(Statement statement)
        {
            var identifier = (TACFunctionIdentifier)statement.RightOperand;

            Function func = null;
            IList<Variable> parameters = null;

            // find the function we are calling
            foreach (var f in functions)
            {  
                if (f.Name == identifier.Name)
                {
                    parameters = f.Parameters;
                    func = f;
                    break;
                }
            }

            if (func == null)
            {
                throw new InternalCompilerError("Failed to find function " + identifier.Name + " when generating function call");
            }

            var args_string = new List<string>();
            var argList = new List<TACValue>();

            for (int i = 0; i < parameters.Count; ++i)
            {
                argList.Add(argStack.Pop());
            }


            for (int i = 0; i < argList.Count; ++i)
            {
                string arg = "";
                var argIsRef = parameters[i].IsReference;
                var paramIsRef = false;
                if (argList[i] is TACIdentifier)
                {
                    var argIdentifier = (TACIdentifier)argList[i];
                    paramIsRef = argIdentifier.IsReference ||
                    capturedVariables.Any(x => x.Identifier.Name == argIdentifier.Name);
                }

                if (argIsRef && !paramIsRef)
                {
                    arg += "&";
                }
                else if (!argIsRef && paramIsRef)
                {
                    arg += "*";
                }

                argList[i].Accept(this);
                arg += cValues.Pop();
                args_string.Add(arg);
            };


            foreach (var captured in func.CapturedVariables)
            {
                args_string.Add(CAPTURE_NOTIFIER + " " +
                    ((capturedVariables.Contains(captured) ||
                        captured.Identifier.IsReference) ?
                        "" : "&") +
                    captured.Identifier.Name);
            }
            var args = string.Join(", ", args_string);

            var destStr = GenerateDestination(statement);
            

            Emit(destStr + identifier.Name + "(" + args + ");");
        }

        void EmitValidateArrayIndex(Statement statement)
        {
            var identifier = (TACIdentifier)statement.LeftOperand;
            statement.RightOperand.Accept(this);
            var indexExpr = cValues.Pop();

            var type = GetCType(identifier.Type);

            var addressOperator = "";
            // captured variables are always references
            if (!(identifier.IsReference ||
                capturedVariables.Any(x => x.Identifier.Name == identifier.Name)))
            {
                addressOperator = "&";
            }

            string indexDeref = "";

            if (statement.RightOperand is TACIdentifier)
            {
                var ident = (TACIdentifier)statement.RightOperand;
                if (ident.IsReference || capturedVariables.Any(x => x.Identifier.Name == ident.Name))
                {
                    indexDeref = "*";
                }

            }

            indexExpr = indexDeref + indexExpr;
            Emit("__validate_" + type + "_array_index(" + 
                addressOperator + "" + identifier.Name + ", " + indexExpr + ", " + (identifier.Line + 1) + ");");

        }

        string getLabel(int labelId)
        {
            return C_LABEL_PREFIX + labelId;
        }

        public void EmitArrayClone(Statement statement)
        {
            var source = (TACIdentifier)statement.RightOperand;
            var destination = (TACIdentifier)statement.Destination;
            var sizeExpr = source.Name;

            if (source.IsReference)
            {
                sizeExpr += "->";
            }
            else
            {
                sizeExpr += ".";
            }
            sizeExpr += "size";

            Emit(GetBaseCType(source.Type) + "_array " + destination.Name + ";");
            declared.Add(destination.Name);
            Emit(ArrayCopy(source, destination, GetBaseCType(source.Type)) + ";");
        }


        private string ArrayCreation(string name, string type, string size, string line)
        {
            Emit(type + "_array " + name + ";");
            return ("__create_" + type + "_array(&" + name + ", " + size + ", " + line + ")");
        }

        private string ArrayCopy(TACIdentifier source, TACIdentifier destination, string type)
        {
            var sourceRefSymbol = "&";
            var destRefSymbol = "&";

            if (source.IsReference || capturedVariables.Any(x => x.Identifier.Name == source.Name))
            {
                sourceRefSymbol = "";
            }

            if (destination.IsReference || capturedVariables.Any(x => x.Identifier.Name == destination.Name))
            {
                destRefSymbol = "";
            }

            return ("__copy_" + type + "_array(" + sourceRefSymbol + source.Name + ", " + destRefSymbol + destination.Name + ")");
        }

        private string GetCType(string minipascalType)
        {
            switch (minipascalType)
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

            return C_VOID;
        }

        private string GetBaseCType(string miniPLType)
        {
            switch (miniPLType)
            {
                case SemanticChecker.INTEGER_ARRAY:
                    return C_INTEGER;
                case SemanticChecker.REAL_ARRAY:
                    return C_REAL;
                case SemanticChecker.STRING_ARRAY:
                    return C_STRING;
                case SemanticChecker.BOOLEAN_ARRAY:
                    return C_BOOLEAN;

                default:
                    throw new InternalCompilerError("Should not be reached");
            }
        }

        string GetDereferenceOperator(TACValue value)
        {
            if (value is TACIdentifier)
            {
                var ident = (TACIdentifier)value;
                if (!ident.IsArray && (ident.IsReference ||
                    capturedVariables.Any(x => x.Identifier.Name == ident.Name)))
                {
                    return "*";
                }
            }

            return "";
        }

    }
}
