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

        private const string C_OBJ_TYPE_NONE = "TYPE_NONE";
        private const string C_OBJ_TYPE_STRING_ARRAY = "TYPE_STRING_ARRAY";

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
        private ISet<Parameter> capturedVariables;
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
            EmitGC();
            EmitArrayStruct(C_INTEGER);
            EmitArrayStruct(C_REAL);
            EmitArrayStruct(C_BOOLEAN);
            EmitArrayStruct(C_STRING);
            EmitStringFunctions();
            EmitAssert();
            Emit("");
            Emit("/***** END OF AUTO-GENERATED HELPER CODE *****/");
            Emit("");
        }

        private void EmitGC()
        {
            Emit(@"
enum Type { " + C_OBJ_TYPE_NONE + ", " + C_OBJ_TYPE_STRING_ARRAY +  @"};

#ifndef GC_DISABLE

#ifndef MAX_HEAP_SIZE 
// 8MB heap by default
#define MAX_HEAP_SIZE 1024*1024*8
#endif

#ifdef GC_DEBUG
#define GC_PRINT(args...) printf(args)
#else
#define GC_PRINT(...)
#endif

typedef struct allocation {
    void *ptr;
    size_t allocation_size;
    struct allocation *next;
    struct allocation *prev;
    char mark;
    enum Type type;
} allocation_t;

typedef struct {
    size_t allocated;
    void *stack_sentinel;
    allocation_t *head;
} GC_State;

GC_State gc_state;

void add_allocation(void *ptr, size_t size, enum Type type)
{
    GC_PRINT(""add_allocation(% p, % d)\n"", ptr, size);
    allocation_t * new_alloc = malloc(sizeof(allocation_t));
    new_alloc->ptr = ptr;
    new_alloc->allocation_size = size;
    new_alloc->mark = 0;
    new_alloc->type = type;

    new_alloc->prev = NULL;
    if (gc_state.head == NULL)
    {
        new_alloc->next = NULL;
        gc_state.head = new_alloc;
    }
    else
    {
        new_alloc->next = gc_state.head;
        gc_state.head->prev = new_alloc;
        gc_state.head = new_alloc;
    }

    gc_state.allocated += size;
}

void free_allocation(allocation_t* allocation)
{
    GC_PRINT(""remove_allocation(%p)\n"", allocation->ptr);

    if (allocation->next != NULL)
    {
        allocation->next->prev = allocation->prev;
    }
    if (allocation->prev != NULL)
    {
        allocation->prev->next = allocation->next;
    }

    if (allocation == gc_state.head)
    {
        gc_state.head = allocation->next;
    }
    gc_state.allocated -= allocation->allocation_size;
    free(allocation->ptr);
    free(allocation);
}


void gc_get_data(int* num_allocs, int* bytes_allocs)
{
    if (gc_state.head == NULL)
    {
        *num_allocs = 0;
        *bytes_allocs = 0;
        return;
    }

    *num_allocs = 1;
    allocation_t* cur = gc_state.head;
    *bytes_allocs = gc_state.head->allocation_size;

    while (cur->next != NULL)
    {
        cur = cur->next;
        *num_allocs += 1;
        *bytes_allocs += cur->allocation_size;
    }
}

void gc_init(void* stack_sentinel)
{
    GC_PRINT(""Initializing GC\n"");
    GC_PRINT(""MAX_HEAP_SIZE: %d\n"", MAX_HEAP_SIZE);
    gc_state.allocated = 0;
    gc_state.stack_sentinel = stack_sentinel;
    gc_state.head = NULL;
}

void gc_scan_stack();
void gc_scan_registers();
void gc_scan_graph_from(const allocation_t*);
void gc_sweep();

void gc_collect()
{
    GC_PRINT(""Collecting dead objects\n"");
    GC_PRINT(""Memory in use: %d bytes\n"", gc_state.allocated);
    if (gc_state.head == NULL)
    {
        GC_PRINT(""No allocated objects - stopping\n"");
        return;
    }
    gc_scan_stack();
    gc_scan_registers();
    gc_sweep();
    GC_PRINT(""GC finished\n"");
    GC_PRINT(""Memory in use: %d bytes\n"", gc_state.allocated);
}

void gc_scan_stack()
{
    GC_PRINT(""Scanning stack\n"");
    void* pos = __builtin_frame_address(0);
    while (pos <= gc_state.stack_sentinel)
    {
        void* as_ptr = *((void**)pos);
        allocation_t* alloc = gc_state.head;
        while (alloc != NULL)
        {
            if (alloc->ptr == as_ptr)
            {
                gc_scan_graph_from(alloc);
                alloc->mark = 1;
            }
            alloc = alloc->next;
        }

        // I'm not sure if GCC guarantees that stack is aligned.
        // if it is, we could use sizeof(void *) here instead.
        // I'm taking conservative stance to ensure correctness
        pos += 1;
    }
}

void gc_scan_registers()
{
    GC_PRINT(""Scanning registers\n"");
    long long registers[16];
    asm(""\t movq %%rax,%0"" : ""=r""(registers[0]));
    asm(""\t movq %%rbx,%0"" : ""=r""(registers[1]));
    asm(""\t movq %%rcx,%0"" : ""=r""(registers[2]));
    asm(""\t movq %%rdx,%0"" : ""=r""(registers[3]));
    asm(""\t movq %%rsi,%0"" : ""=r""(registers[4]));
    asm(""\t movq %%rdi,%0"" : ""=r""(registers[5]));
    asm(""\t movq %%rbp,%0"" : ""=r""(registers[6]));
    asm(""\t movq %%rsp,%0"" : ""=r""(registers[7]));
    asm(""\t movq %%r8,%0"" : ""=r""(registers[8]));
    asm(""\t movq %%r9,%0"" : ""=r""(registers[9]));
    asm(""\t movq %%r10,%0"" : ""=r""(registers[10]));
    asm(""\t movq %%r11,%0"" : ""=r""(registers[11]));
    asm(""\t movq %%r12,%0"" : ""=r""(registers[12]));
    asm(""\t movq %%r13,%0"" : ""=r""(registers[13]));
    asm(""\t movq %%r14,%0"" : ""=r""(registers[14]));
    asm(""\t movq %%r15,%0"" : ""=r""(registers[15]));

    int i = 0;
    for (; i < 16; ++i)
    {
        allocation_t* alloc = gc_state.head;
        while (alloc != NULL)
        {
            if (alloc->ptr == (void*)registers[i])
            {
                gc_scan_graph_from(alloc);
                alloc->mark = 1;
            }
            alloc = alloc->next;
        }
    }
}

void gc_scan_graph_from(const allocation_t* cur)
{
    if (cur->mark == 1)
    {
        GC_PRINT(""Object already marked - not scanning"");
        return;
    }
    
    if (cur->type == TYPE_STRING_ARRAY)
    {
        GC_PRINT(""String array - scanning\n"");
    string* arr = cur->ptr;
    size_t size = cur->allocation_size / sizeof(string*);
    int i = 0;
        for (; i<size; ++i)
        {
            allocation_t* alloc = gc_state.head;
            while (alloc != NULL)
            {
                if (alloc->ptr == arr[i])
                {
                    alloc->mark = 1;
                }
                alloc = alloc->next;
            }
        }
    }
}

void gc_sweep()
{
    allocation_t* alloc = gc_state.head;
    while (alloc != NULL)
    {
        allocation_t* cur = alloc;
        alloc = cur->next;
        if (cur->mark == 0)
        {
            free_allocation(cur);
        }
        else
        {
            cur->mark = 0;
        }
    }
}
#endif
void* gc_malloc(size_t size, enum Type type)
{
    #ifndef GC_DISABLE
    if (gc_state.allocated + size > MAX_HEAP_SIZE)
    {
        gc_collect();
        if (gc_state.allocated +  size > MAX_HEAP_SIZE)
        {
            fprintf(stderr, ""Out of memory"");
            exit(1);
        }
    }
    #endif
    void* ptr = malloc(size);

    # ifndef GC_DISABLE
    add_allocation(ptr, size, type);
    #endif
    return ptr;
}

void* gc_calloc(size_t num, size_t size, enum Type type)
{
    #ifndef GC_DISABLE
    if (gc_state.allocated + num* size > MAX_HEAP_SIZE)
    {
        gc_collect();
        if (gc_state.allocated + num* size > MAX_HEAP_SIZE)
        {
            fprintf(stderr, ""Out of memory"");
            exit(1);
        }
    }
    #endif
    void* ptr = calloc(num, size);
    #ifndef GC_DISABLE
    add_allocation(ptr, num* size, type);
    #endif
    return ptr;
}


");
        }

        private void EmitArrayStruct(string type, string gc_type=C_OBJ_TYPE_NONE)
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
    dst->arr = gc_malloc(src->size * sizeof(" + type + @"), " + C_OBJ_TYPE_NONE + @");
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
    char *dst = gc_malloc(size, " + C_OBJ_TYPE_NONE + @");
 
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
}
");
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
            if (function.Name == TACGenerator.ENTRY_POINT)
            {
                Emit("int main()");
            }
            else
            {
                var param_list = new List<string>(function.Parameters.Select(x => 
                GetCType(x.Type) + " " +( x.IsReference ? "*" : "") + x.Identifier.Name));

                param_list.AddRange(
                    function.CapturedVariables.Select(x => GetCType(x.Type) + " *" + x.Identifier.Name));

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

                dest += 
                    GetDereferenceOperator(statement.Destination) + 
                    cValues.Pop() + " = ";
            }

            if (statement.LeftOperand != null)
            {
                statement.LeftOperand.Accept(this);
                lhs += GetDereferenceOperator(statement.LeftOperand) + cValues.Pop();
            }

            if (statement.RightOperand != null)
            {
                statement.RightOperand.Accept(this);
                rhs += GetDereferenceOperator(statement.RightOperand) +  cValues.Pop();
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
            // as long as they aren't globals
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
            // captured variables are always references
            if (tacArrayIndex.IsReference || 
                capturedVariables.Any(x => x.Identifier.Name == tacArrayIndex.Name))
            {
                memberOperator = "->";
            }
            else
            {
                addressOperator = "&";
            }

            Emit("__validate_" + type + "_array_index(" + addressOperator + "" + tacArrayIndex.Name + ", " + index + ", " + (tacArrayIndex.Line + 1) + ");");
            cValues.Push(tacArrayIndex.Name + memberOperator + "arr[" + index + "]");
        }

        public void Visit(TACArrayDeclaration tacArrayDeclaration)
        {
            var type = GetCType(tacArrayDeclaration.Type);
            var name = tacArrayDeclaration.Name;
            var size = tacArrayDeclaration.Expression;

            cValues.Push(ArrayCreation(name, type, size.ToString(), (tacArrayDeclaration.Line + 1).ToString()));

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
                var prefix = GetDereferenceOperator(arg);
                argumentList.Add(prefix + cValues.Pop());
                specifierList.Add(formatSpecifiers[GetCType(arg)]);
            }

            cValues.Push("printf(\"" + string.Join("", specifierList) + "\\n\", " + string.Join(", ", argumentList) + ")");
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
            tacArraySize.Array.Accept(this);
            var memberOp = ".";
            if (tacArraySize.Array is TACIdentifier)
            {
                var ident = (TACIdentifier)tacArraySize.Array;
                if (ident.IsReference ||
                    capturedVariables.Any(x => x.Identifier.Name == ident.Name))
                {
                    memberOp = "->";
                }
            }
            cValues.Push(cValues.Pop() + memberOp + "size");
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

            Function func = null;
            IList<Parameter> parameters = null;
            foreach (var function in functions)
            {
                if (function.Name == tacCall.Function)
                {
                    parameters = function.Parameters;
                    func = function;
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
                    paramIsRef = identifier.IsReference ||
                    capturedVariables.Any(x => x.Identifier.Name == identifier.Name);
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


            foreach (var captured in func.CapturedVariables)
            {
                args_string.Add(
                    ((capturedVariables.Contains(captured) || 
                        captured.Identifier.IsReference) ? 
                        "" : "&") +
                    captured.Identifier.Name);
            }
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
            tacAssert.Expression.Accept(this);
            cValues.Push("assert(" + cValues.Pop() +  ", " + (tacAssert.Line + 1) + ")");
        }

        public void Visit(TACCallRead tacCallRead)
        {
            throw new NotImplementedException();
        }

        public void Visit(TACCloneArray tacCloneArray)
        {
            var source = tacCloneArray.Source;
            var destination = tacCloneArray.Destination;
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
            cValues.Push(ArrayCopy(source, destination, GetBaseCType(source.Type)));
        }


        private string ArrayCreation(string name, string type, string size, string line)
        {
            Emit(type + "_array " + name + ";");
            return ("__create_" + type + "_array(&" + name + ", " + size + ", " + line +")");
        }

        private string ArrayCopy(TACIdentifier source, TACIdentifier destination, string type)
        {
            var sourceRefSymbol = "&";
            var destRefSymbol = "&";

            if (source.IsReference)
            {
                sourceRefSymbol = "";
            }

            if (destination.IsReference)
            {
                destRefSymbol = "";
            }

            return ("__copy_" + type + "_array(" + sourceRefSymbol + source.Name + ", " + destRefSymbol +  destination.Name +")");
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
            if (v is TACInteger || v is TACArraySize)
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
                if (ident.IsReference ||
                    capturedVariables.Any(x => x.Identifier.Name == ident.Name))
                {
                    return "*";
                }
            }

            return "";
        }

    }
}
