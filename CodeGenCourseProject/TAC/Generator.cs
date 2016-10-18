using CodeGenCourseProject.AST;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using static CodeGenCourseProject.TAC.Function;

namespace CodeGenCourseProject.TAC
{
    public enum Operator
    {
        PLUS, MINUS, MULTIPLY, DIVIDE, MODULO, CONCAT,
        LESS_THAN, LESS_THAN_OR_EQUAL, EQUAL, GREATER_THAN_OR_EQUAL, GREATER_THAN, NOT_EQUAL, AND, OR, NOT,
        PUSH, PUSH_INITIALIZED, CALL, CALL_WRITELN, CALL_READ, CALL_ASSERT, LABEL, JUMP, JUMP_IF_FALSE, RETURN,
        VALIDATE_INDEX, ARRAY_SIZE, CLONE_ARRAY, DECLARE_ARRAY,
    };

    public static class OperatorExtension
    {
        public static string Name(this Operator op)
        {
            switch (op)
            {
                case Operator.PLUS:
                    return "+";
                case Operator.MINUS:
                    return "-";
                case Operator.MULTIPLY:
                    return "*";
                case Operator.DIVIDE:
                    return "/";
                case Operator.MODULO:
                    return "%";
                case Operator.CONCAT:
                    return "<concat>";
                case Operator.LESS_THAN:
                    return "<";
                case Operator.LESS_THAN_OR_EQUAL:
                    return "<=";
                case Operator.EQUAL:
                    return "==";
                case Operator.GREATER_THAN_OR_EQUAL:
                    return ">=";
                case Operator.GREATER_THAN:
                    return ">";
                case Operator.NOT_EQUAL:
                    return "!=";
                case Operator.AND:
                    return "&&";
                case Operator.OR:
                    return "||";
                case Operator.NOT:
                    return "!";
                case Operator.PUSH:
                    return "push";
                case Operator.PUSH_INITIALIZED:
                    return "push_initialized";
                case Operator.CALL:
                    return "call";
                case Operator.CALL_WRITELN:
                    return "call_writeln";
                case Operator.CALL_READ:
                    return "call_read";
                case Operator.CALL_ASSERT:
                    return "call_assert";
                case Operator.LABEL:
                    return "label";
                case Operator.JUMP:
                    return "jump";
                case Operator.JUMP_IF_FALSE:
                    return "jump_if_false";
                case Operator.RETURN:
                    return "return";
                case Operator.VALIDATE_INDEX:
                    return "validate_index";
                case Operator.ARRAY_SIZE:
                    return "array_size";
                default:
                    throw new InternalCompilerError("Default branch taken in Operator.Name()");
            }
        }
    }

    public class Generator : ASTVisitor
    {
        private string programName;
        private readonly IList<Function> functions;
        // working stack; functions will be eventually moved into the function list
        private readonly Stack<Function> functionStack;
        public const string ENTRY_POINT = "<ENTRY POINT>";
        private int tempID;
        private int labelID;
        private SymbolTable symbolTable;
        private Stack<TACValue> tacValueStack; // used to store temporaries when evaluating sub-expressions


        // track identifiers that have been captured from the outer scope
        // (not function local variables or parameters)
        private Stack<ISet<Variable>> capturedIdentifiers;
        // contains outer symbol tables (symbol table without current function locals etc.)
        // used to identify captured variables
        private Stack<SymbolTable> outerSymbolTables;

        public Generator()
        {
            functions = new List<Function>();
            functionStack = new Stack<Function>();
            tacValueStack = new Stack<TACValue>();
            symbolTable = new SymbolTable(null);

            outerSymbolTables = new Stack<SymbolTable>();
            capturedIdentifiers = new Stack<ISet<Variable>>();
            tempID = 0;
            labelID = 0;
        }

        public IList<Function> Functions
        {
            get
            {
                return functions;
            }
        }

        public string ProgramName
        {
            get
            {
                return programName;
            }
        }

        public void Visit(ArrayIndexNode node)
        {
            var nameNode = (IdentifierNode)node.Children[0];

            nameNode.Accept(this);
            tacValueStack.Pop();

            var name = nameNode.Value;
            var expr = node.Children[1];
            expr.Accept(this);
            var exprTAC = tacValueStack.Pop();
            var arrayIsReference = false;
            var symbol = (ArraySymbol)symbolTable.GetSymbol(name);

            // if the variable exists in the symbol table but has not been declared on this level, it is captured


            arrayIsReference = symbol.IsReference;
            var addressing = AddressingMode.NONE;

            var exprId = exprTAC as TACIdentifier;
            if (exprId != null && exprId.IsPointer)
            {
                exprId.AddressingMode = AddressingMode.DEREFERENCE;
            }
            bool isCaptured = IsCaptured(name, symbol.Id);

            // validation expects a reference, so if symbol is not a reference, we must take the address of the symbol
            if (!arrayIsReference && !isCaptured)
            {
                addressing = AddressingMode.TAKE_ADDRESS;
            }

            var validationName = new TACIdentifier(nameNode.Line, nameNode.Column, name, symbol.BaseType, symbol.Id, addressing);
            Emit(Operator.VALIDATE_INDEX, validationName, exprTAC, null);

            var destination = GetTemporary(node);
            destination.AddressingMode = AddressingMode.DEREFERENCE;
            destination.IsPointer = true;



            var arrayName = new TACIdentifier(nameNode.Line, nameNode.Column, name, symbol.BaseType, symbol.Id, addressing);
            arrayName.IsArray = true;
            arrayName.IsPointer = arrayIsReference || isCaptured;
            Emit(Operator.PLUS, arrayName, exprTAC, destination);
            tacValueStack.Push(destination);
        }

        public void Visit(ArrayTypeNode arrayTypeNode)
        {
            TACValue arraySize;
            if (arrayTypeNode.Children.Count == 2)
            {
                arrayTypeNode.Children[1].Accept(this);
                arraySize = tacValueStack.Pop();
            }
            else
            {
                arraySize = new TACInteger(arrayTypeNode.Line, arrayTypeNode.Column, 0);
            }

            tacValueStack.Push(arraySize);
        }

        public void Visit(BlockNode blockNode)
        {
            symbolTable.PushLevel(blockNode.GetSymbols());
            foreach (var child in blockNode.Children)
            {
                child.Accept(this);
                // pop unused return value from stack, if present
                if (child is CallNode && tacValueStack.Count != 0)
                {
                    tacValueStack.Pop();
                }
            }

            symbolTable.PopLevel();
        }


        // TODO: Clean up, it is horribly messy
        // May want to rethink variable capture handling, most of the complexity 
        // comes from that (add to argument list earlier?)
        public void Visit(CallNode callNode)
        {
            var name = ((IdentifierNode)callNode.Children[0]).Value;
            var arguments = new List<TACValue>();

            // is null when handling inbuilt functions like writeln
            var functionSymbol = (FunctionSymbol)symbolTable.GetSymbol(name, callNode.Line, callNode.Column);

            for (int i = 1; i < callNode.Children.Count; ++i)
            {
                callNode.Children[i].Accept(this);

                // if argument is array, and the corresponding parameter is not
                // a reference parameter, we must generate a defensive copy 
                // so that any modifications in the called function do not 
                // affect the original array 
                var argument = tacValueStack.Pop();
                if (functionSymbol != null && argument is TACIdentifier)
                {
                    argument = GenerateCopyIfNeeded(callNode, functionSymbol, i, argument);
                }

                arguments.Add(argument);
            }

            // handle captured variables
            if (functionSymbol != null)
            {
                foreach (var f in functions)
                {
                    if (f.Name == Helper.MangleFunctionName(functionSymbol.Name, functionSymbol.Id))
                    {
                        var list = new List<Variable>(f.CapturedVariables);
                        for (int i = list.Count - 1; i >= 0; --i)
                        {

                            var id = new TACIdentifier(list[i].Identifier);
                            id.Line = callNode.Line;
                            id.Column = callNode.Column;
                            var symbol = symbolTable.GetSymbol(id.UnmangledName) as VariableSymbol;

                            var isCaptured = IsCaptured(id.UnmangledName, id.Id);
                            if (isCaptured || (symbol != null && symbol.IsReference))
                            {
                                id.AddressingMode = AddressingMode.NONE;
                            }
                            else
                            {
                                id.AddressingMode = AddressingMode.TAKE_ADDRESS;
                            }
                            id.IsCaptured = true;
                            Emit(Operator.PUSH, null, id, null);
                        }

                        break;
                    }
                }
            }

            var pushType = Operator.PUSH;
            bool isInbuiltRead = false;
            bool isInbuiltWriteln = false;
            if (name == "read" && functionSymbol == null)
            {
                pushType = Operator.PUSH_INITIALIZED;
                isInbuiltRead = true;
            }

            if (name == "writeln" && functionSymbol == null)
            {
                isInbuiltWriteln = true;
            }


            // push in reverse order, as this makes code gen slightly easier
            for (int i = arguments.Count - 1; i >= 0; --i)
            {
                if (arguments[i] is TACIdentifier)
                {
                    var isRefefenceArgument = false;

                    // if function argument is reference, make sure addressing mode is correct
                    if (isInbuiltRead || functionSymbol != null && functionSymbol.IsReferenceParameters[i])
                    {
                        isRefefenceArgument = true;
                    }

                    var identifier = (TACIdentifier)arguments[i];
                    if (identifier.IsPointer && isRefefenceArgument || (!identifier.IsPointer && !isRefefenceArgument))
                    {
                        identifier.AddressingMode = AddressingMode.NONE;
                    }
                    else if (!identifier.IsPointer && isRefefenceArgument)
                    {
                        identifier.AddressingMode = AddressingMode.TAKE_ADDRESS;
                    }
                    else if (identifier.IsPointer && !isRefefenceArgument)
                    {
                        identifier.AddressingMode = AddressingMode.DEREFERENCE;
                    }
                }
                Emit(pushType, null, arguments[i], null);
            }


            // inbuilt read function
            if (isInbuiltRead)
            {   Emit(Operator.CALL_READ, null, new TACInteger(arguments.Count), null);
                return;
            }

            // inbuilt writeln function
            if (isInbuiltWriteln)
            {
                Emit(Operator.CALL_WRITELN, null, new TACInteger(arguments.Count), null);
                return;
            }



            /*
                If the called function has captured variables, which are not locals for current function and are not used
                in the function, we need to capture them as well for this function.
                This ensures that the captured variables get properly passed around when creating function calls        
            */
            if (functionStack.Count > 1) // Ignores the program node and inbuilt functions
            {
                Function f = null;
                foreach (var func in functions)
                {
                    if (func.Name == Helper.MangleFunctionName(name, functionSymbol.Id))
                    {
                        f = func;
                        break;
                    }
                }

                if (f != null)
                {
                    foreach (var captured in f.CapturedVariables)
                    {
                        if (!functionStack.Peek().Locals.Contains(captured) &&
                            !functionStack.Peek().Parameters.Contains(captured))
                        {
                            // we know at this point that the captured variable is a pointer, since it comes from outer scope
                            // we also know that the next function expects a pointer, so we do not need any addressing operations
                            var newId = new TACIdentifier(captured.Identifier);
                            var newVar = new Variable(newId, captured.Type, true);

                            functionStack.Peek().
                                CapturedVariables.
                                Add(newVar);
                        }
                    }

                }
            }

            if (callNode.NodeType() == SemanticChecker.VOID_TYPE)
            {
                Emit(Operator.CALL, null, new TACFunctionIdentifier(callNode.Line, callNode.Column, name, functionSymbol.Id), null);
            }
            else
            {
                var temp = GetTemporary(callNode);
                Emit(Operator.CALL, null, new TACFunctionIdentifier(callNode.Line, callNode.Column, name, functionSymbol.Id), temp);
                tacValueStack.Push(temp);
            }
        }

        private TACValue GenerateCopyIfNeeded(CallNode callNode, FunctionSymbol functionSymbol, int i, TACValue argument)
        {
            var argumentIdentifier = (TACIdentifier)argument;
            var argumentSymbol = symbolTable.GetSymbol(
                                    argumentIdentifier.UnmangledName, callNode.Line, callNode.Column);
            if (!(argumentSymbol is ArraySymbol))
            {
                return argument;
            }
            // reference parameters are not copied
            var functionArgumentIsReference = functionSymbol.IsReferenceParameters[i - 1];
            if (functionArgumentIsReference)
            {
                return argument;
            }

            var temporary = GetTemporary(callNode.Children[i]);

            var addressing = AddressingMode.NONE;

            // array copy helper function expects a pointer, so if the original pointer does not have 
            // dereference-addressing mode (not a pointer), we must take its address
            if (!argumentIdentifier.IsPointer)
            {
                addressing = AddressingMode.TAKE_ADDRESS;
            }

            var copyIdentifier = new TACIdentifier(argumentIdentifier);
            copyIdentifier.AddressingMode = addressing;

            temporary.AddressingMode = AddressingMode.TAKE_ADDRESS;

            Emit(Operator.CLONE_ARRAY, null, copyIdentifier, temporary);

            var retIdentifier = new TACIdentifier(temporary);
            retIdentifier.AddressingMode = AddressingMode.NONE;
            return retIdentifier;
        }

        public void Visit(EqualsNode equalsNode)
        {
            HandleBinaryOperator(equalsNode, Operator.EQUAL);
        }


        public void Visit(ReturnNode returnNode)
        {
            AssertEmptyTacValueStack();
            if (returnNode.Children.Count == 0)
            {
                Emit(Operator.RETURN, null, null, null);
                return;
            }
            returnNode.Children[0].Accept(this);
            var variable = tacValueStack.Pop();
            var varId = variable as TACIdentifier;

            if (varId != null && varId.IsPointer)
            {
                varId.AddressingMode = AddressingMode.DEREFERENCE;
            }

            Emit(Operator.RETURN, null, variable, null);
            AssertEmptyTacValueStack();
        }

        public void Visit(SubtractNode subtractNode)
        {
            HandleBinaryOperator(subtractNode, Operator.MINUS);
        }

        public void Visit(OrNode orNode)
        {
            HandleBinaryOperator(orNode, Operator.OR);
        }

        public void Visit(NotNode notNode)
        {
            HandleUnaryOperator(notNode, Operator.NOT);
        }

        public void Visit(StringNode stringNode)
        {
            tacValueStack.Push(
                new TACString(stringNode.Line, stringNode.Column, stringNode.Value));
        }

        public void Visit(NegateNode negateNode)
        {
            HandleUnaryOperator(negateNode, Operator.MINUS);
        }

        public void Visit(ModuloNode moduloNode)
        {
            HandleBinaryOperator(moduloNode, Operator.MODULO);
        }

        public void Visit(LessThanNode lessThanNode)
        {
            HandleBinaryOperator(lessThanNode, Operator.LESS_THAN);
        }

        public void Visit(LessThanOrEqualNode lessThanOrEqualNode)
        {
            HandleBinaryOperator(lessThanOrEqualNode, Operator.LESS_THAN_OR_EQUAL);
        }

        public void Visit(IdentifierNode identifierNode)
        {
            var name = identifierNode.Value;
            var symbol = symbolTable.GetSymbol(name, identifierNode.Line, identifierNode.Column);

            // if symbol is null, it's predeclared identifier and we need to check for true/false
            // if the symbol is "true" or "false", it might still be a predefined identifier, as 
            // the symbol table works at block scope. This means that for declarations like
            //
            // begin
            //   a := true
            //   var true : integer;
            // end;
            //  
            // the true-identifier in a would show up as symbol, even if it is the boolean constant in this
            // context
            //
            if (symbol == null || symbol.Name == "true" || symbol.Name == "false")
            {

                // if the symbol is null, or it is declared after the identifier, the identifier
                // is predeclared identifier
                if (symbol == null || symbol.Line > identifierNode.Line)
                {
                    switch (name)
                    {
                        case "true":
                            tacValueStack.Push(
                                new TACBoolean(identifierNode.Line, identifierNode.Column, true));
                            break;
                        case "false":
                            tacValueStack.Push(
                                new TACBoolean(identifierNode.Line, identifierNode.Column, false));
                            break;
                        default:
                            break;
                    }
                    return;
                }
            }

            var outerSymbol =
                outerSymbolTables.Count > 0 ?
                    outerSymbolTables.Peek().GetSymbol(name, identifierNode.Line, identifierNode.Column)
                    : null;

            // if symbol is the same symbol than one in the outer symbol table 
            // (table without current function declarations), the variable is captured
            // from outer context. Make sure it is treated as a pointer
            TACIdentifier identifier = null;
            if (symbol == outerSymbol)
            {
                identifier = new TACIdentifier(
                     identifierNode.Line, identifierNode.Column, name, symbol.Type, symbol.Id);
                identifier.IsPointer = true;
                capturedIdentifiers.Peek().Add(
                    new Variable(
                        identifier,
                        symbol.Type,
                        true));
            }
            else
            {
                var isPointer = false;
                if (symbol is VariableSymbol)
                {
                    isPointer = ((VariableSymbol)symbol).IsReference;
                }
                else if (symbol is ArraySymbol)
                {
                    isPointer = ((ArraySymbol)symbol).IsReference;
                }
                
                identifier = new TACIdentifier(
                     identifierNode.Line, identifierNode.Column, name, symbol.Type, symbol.Id);
                identifier.IsPointer = isPointer;
            }


            tacValueStack.Push(identifier);
        }

        public void Visit(IntegerNode integerNode)
        {
            tacValueStack.Push(new TACInteger(integerNode.Line, integerNode.Column, integerNode.Value));
        }

        public void Visit(IfNode ifNode)
        {
            AssertEmptyTacValueStack();
            var expression = ifNode.Children[0];
            var ifBlock = ifNode.Children[1];
            var hasElseBlock = ifNode.Children.Count == 3;

            TACInteger endIfBlock = null;
            TACInteger endElseBlock = null;

            endIfBlock = new TACInteger(GetLabelId());
            if (hasElseBlock)
            {
                endElseBlock = new TACInteger(GetLabelId());
            }

            expression.Accept(this);
            var condition = tacValueStack.Pop();

            Emit(Operator.JUMP_IF_FALSE, condition, endIfBlock, null);
            ifBlock.Accept(this);

            if (hasElseBlock)
            {
                Emit(Operator.JUMP, null, endElseBlock, null);
            }

            Emit(Operator.LABEL, null, endIfBlock, null);

            if (hasElseBlock)
            {
                var elseBlock = ifNode.Children[2];
                elseBlock.Accept(this);
                Emit(Operator.LABEL, null, endElseBlock, null);
            }
            AssertEmptyTacValueStack();
        }

        public void Visit(GreaterThanNode greaterThanNode)
        {
            HandleBinaryOperator(greaterThanNode, Operator.GREATER_THAN);
        }

        public void Visit(ProgramNode programNode)
        {
            programName = programNode.Name;
            var function = new Function(
                programNode.Line,
                programNode.Column,
                ENTRY_POINT,
                1,
                SemanticChecker.VOID_TYPE);
            functionStack.Push(function);
            foreach (var child in programNode.Children)
            {
                child.Accept(this);
            }

            Functions.Add(functionStack.Pop());
        }

        public void Visit(RealNode realNode)
        {
            tacValueStack.Push(new TACReal(realNode.Line, realNode.Column, realNode.Value));
        }

        public void Visit(GreaterThanOrEqualNode greaterThanOrEqualNode)
        {
            HandleBinaryOperator(greaterThanOrEqualNode, Operator.GREATER_THAN_OR_EQUAL);
        }

        public void Visit(ProcedureNode procedureNode)
        {
            var block = 1;
            var paramStart = 2;
            ProcedureFunctionHelper(procedureNode, SemanticChecker.VOID_TYPE, block, paramStart);
        }

        public void Visit(FunctionNode functionNode)
        {
            var block = 2;
            var paramStart = 3;
            var type = "";
            if (functionNode.Children[1] is IdentifierNode)
            {
                type = ((IdentifierNode)functionNode.Children[1]).Value;
            }
            else
            {
                type =
                    SemanticChecker.ARRAY_PREFIX +
                    ((IdentifierNode)((ArrayTypeNode)functionNode.Children[1]).Children[0]).Value +
                    SemanticChecker.ARRAY_SUFFIX;
            }

            ProcedureFunctionHelper(
                functionNode,
                type,
                block,
                paramStart);
        }

        public void Visit(MultiplyNode multiplyNode)
        {
            HandleBinaryOperator(multiplyNode, Operator.MULTIPLY);
        }

        public void Visit(FunctionParameterVariableNode functionParameterNode)
        {
            var symbol = symbolTable.GetSymbol(functionParameterNode.Name.Value);
            var id = new TACIdentifier(symbol.Line, symbol.Column, symbol.Name, symbol.Type, symbol.Id);
            
            functionStack.Peek().
                AddParameter(
                    id,
                    functionParameterNode.NodeType(),
                    functionParameterNode.IsReferenceParameter);
        }

        public void Visit(FunctionParameterArrayNode functionParameterArrayNode)
        {

            var symbol = symbolTable.GetSymbol(functionParameterArrayNode.Name.Value);
            var id = new TACIdentifier(symbol.Line, symbol.Column,
                symbol.Name, symbol.Type, symbol.Id);

            functionStack.Peek().
                AddParameter(
                    id,
                    functionParameterArrayNode.NodeType(),
                    functionParameterArrayNode.IsReferenceParameter);
        }

        public void Visit(VariableAssignmentNode variableAssignmentNode)
        {
            AssertEmptyTacValueStack();
            foreach (var child in variableAssignmentNode.Children)
            {
                child.Accept(this);
            }

            var expr = tacValueStack.Pop();
            var store = tacValueStack.Pop();

            // ensure store has correct addressing mode
            var id = (TACIdentifier)store;
            
            if (id.IsPointer)
            {
                id.AddressingMode = AddressingMode.DEREFERENCE;
            }

            // ensure expression has correct addressing mode
            
            var exprId = expr as TACIdentifier;
            if (exprId != null && exprId.IsPointer)
            {
                exprId.AddressingMode = AddressingMode.DEREFERENCE; 
            }

            Emit(
                expr,
                store);
            AssertEmptyTacValueStack();
        }

        public void Visit(VariableDeclarationNode variableDeclarationNode)
        {
            AssertEmptyTacValueStack();
            // regular variables can be declared when they are used for the first time, but the
            // array size expression is only present in the variable declaration node, so we
            // need a special node storing this value

            // declarations are added to function local list so that they can be distinquished from
            // captures when analyzing control flow

            if (variableDeclarationNode.Children[0] is ArrayTypeNode)
            {
                variableDeclarationNode.Children[0].Accept(this);
                var arraySize = tacValueStack.Pop();
                for (int i = 1; i < variableDeclarationNode.Children.Count; ++i)
                {
                    var name = (IdentifierNode)variableDeclarationNode.Children[i];
                    var symbol = (ArraySymbol)symbolTable.GetSymbol(name.Value, variableDeclarationNode.Line + 1, variableDeclarationNode.Column);
                    Emit(
                      Operator.DECLARE_ARRAY,
                      new TACIdentifier(name.Line, name.Column, name.Value, symbol.BaseType, symbol.Id),
                      arraySize,
                      null);

                    AssertEmptyTacValueStack();
                    if (symbol.IsReference)
                    {
                        throw new NotImplementedException("Not implemented");
                    }
                    var addressing = AddressingMode.NONE; // TODO - handle corretly

                    functionStack.Peek().Locals.Add(
                        new Variable(
                            new TACIdentifier(name.Value, symbol.Type, symbol.Id, addressing),
                            symbol.Type,
                            symbol.IsReference
                        ));
                }
            }
            else
            {
                for (int i = 1; i < variableDeclarationNode.Children.Count; ++i)
                {
                    var name = (IdentifierNode)variableDeclarationNode.Children[i];
                    var symbol = (VariableSymbol)symbolTable.GetSymbol(name.Value, variableDeclarationNode.Line + 1, variableDeclarationNode.Column);

                    var addressing = AddressingMode.NONE; // TODO - handle corretly
                    if (symbol.IsReference)
                    {
                        throw new NotImplementedException("Not implemented");
                    }

                    functionStack.Peek().Locals.Add(
                        new Variable(
                            new TACIdentifier(name.Value, symbol.Type, symbol.Id, addressing),
                            symbol.Type,
                            symbol.IsReference
                        ));
                }
            }
        }

        public void Visit(NotEqualsNode notEqualsNode)
        {
            HandleBinaryOperator(notEqualsNode, Operator.NOT_EQUAL);
        }

        public void Visit(WhileNode whileNode)
        {
            AssertEmptyTacValueStack();

            var beginLabel = GetLabelId();


            var endLabel = GetLabelId();
            Emit(Operator.LABEL, null, new TACInteger(beginLabel), null);


            whileNode.Children[0].Accept(this);
            var condition = tacValueStack.Pop();

            Emit(Operator.JUMP_IF_FALSE, condition, new TACInteger(endLabel), null);
            whileNode.Children[1].Accept(this);

            Emit(Operator.JUMP, null, new TACInteger(beginLabel), null);
            Emit(Operator.LABEL, null, new TACInteger(endLabel), null);

            AssertEmptyTacValueStack();
        }

        public void Visit(ErrorNode errorNode)
        {
            throw new InternalCompilerError("ErrorNode present when error-free syntax tree is expected");
        }

        public void Visit(DivideNode divideNode)
        {
            HandleBinaryOperator(divideNode, Operator.DIVIDE);
        }

        public void Visit(UnaryPlusNode unaryPlusNode)
        {
            unaryPlusNode.Children[0].Accept(this);
        }

        public void Visit(AssertNode assertNode)
        {
            AssertEmptyTacValueStack();

            assertNode.Children[0].Accept(this);
            var expr = tacValueStack.Pop();
            var id = expr as TACIdentifier;
            if (id != null && id.IsPointer)
            {
                id.AddressingMode = AddressingMode.DEREFERENCE;
            }
            Emit(Operator.CALL_ASSERT, new TACInteger(assertNode.Line), expr, null);
            AssertEmptyTacValueStack();
        }

        public void Visit(ArraySizeNode node)
        {
            node.Children[0].Accept(this);
            var temp = GetTemporary(node);
            Emit(Operator.ARRAY_SIZE, null, tacValueStack.Pop(), temp);
            tacValueStack.Push(temp);
        }

        public void Visit(AndNode node)
        {
            HandleBinaryOperator(node, Operator.AND);
        }

        public void Visit(AddNode node)
        {
            if (node.NodeType() == SemanticChecker.STRING_TYPE)
            {
                HandleBinaryOperator(node, Operator.CONCAT);
            }
            else
            {
                HandleBinaryOperator(node, Operator.PLUS);
            }
        }

        private void HandleUnaryOperator(ASTNode node, Operator op)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this);
            }

            var tacValue = GetTemporary(node);

            var rhs = tacValueStack.Pop();

            var rId = rhs as TACIdentifier;

            if (rId != null && rId.IsPointer)
            {
                rId.AddressingMode = AddressingMode.DEREFERENCE;
            }

            Emit(op, rhs, tacValue);
            tacValueStack.Push(tacValue);
        }

        private TACIdentifier GetTemporary(ASTNode node)
        {
            var curId = tempID++;
            return new TACIdentifier(node.Line, node.Column, "__t", node.NodeType(), curId);
        }

        private int GetLabelId()
        {
            return labelID++;
        }

        private void HandleBinaryOperator(ASTNode node, Operator op)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this);
            }

            var tacValue = GetTemporary(node);

            var rhs = tacValueStack.Pop();
            var lhs = tacValueStack.Pop();

            var rId = rhs as TACIdentifier;
            var lId = lhs as TACIdentifier;

            if (rId != null && rId.IsPointer)
            {
                rId.AddressingMode = AddressingMode.DEREFERENCE;
            }

            if (lId != null && lId.IsPointer)
            {
                lId.AddressingMode = AddressingMode.DEREFERENCE;
            }

            Emit(op, lhs, rhs, tacValue);

            tacValueStack.Push(tacValue);
        }

        private void ProcedureFunctionHelper(ASTNode node, string returnType, int block, int paramStart)
        {
            var identifier = (IdentifierNode)node.Children[0];
            var symbol = symbolTable.GetSymbol(identifier.Value);
            var function = new Function(
                node.Line, node.Column, identifier.Value, symbol.Id, returnType);

            outerSymbolTables.Push(symbolTable.Clone());
            capturedIdentifiers.Push(new HashSet<Variable>());
            functionStack.Push(function);

            // honestly starting to regret how I decided to handle function\procedure nodes in AST.
            // Too many dirty workarounds.
            // this one in particular is because I need the symbol table info here
            // that is only present in the child block
            
            symbolTable.PushLevel(((BlockNode)node.Children[block]).GetSymbols());
            for (int i = paramStart; i < node.Children.Count; ++i)
            {
                node.Children[i].Accept(this);
            }

            node.Children[block].Accept(this);


            symbolTable.PopLevel();
            var captured = capturedIdentifiers.Pop();

            // if inner function captures a value, this value needs to be captured
            // by outer function as well, if this is declared in even outer scope
            //
            // var a : integer;
            // 
            // proc p1(); // captures a, doesn't capture b as that is inner variable
            // begin
            //    var b : integer;
            //    proc p2(); // captures a and b
            //       *use a and b*
            //    end
            // end
            //
            
            // create new copy of stack; two stack creation ensures stack is not upside down
            var fTables =
                new Stack<SymbolTable>(new Stack<SymbolTable>(outerSymbolTables));
            foreach (var f in functionStack)
            {
                if (f.Name == ENTRY_POINT)
                {
                    break;
                }
                
                foreach (var captureParam in captured)
                {
                    var s = fTables.Peek().GetSymbol(
                        captureParam.Identifier.UnmangledName);
                    if (s != null && s.Id == captureParam.Identifier.Id)
                    {
                        f.CapturedVariables.Add(captureParam);
                    }
                }
                fTables.Pop();
            }
            outerSymbolTables.Pop();
            Functions.Add(functionStack.Pop());
        }

        private void Emit(TACValue operand)
        {
            functionStack.Peek().Statements.Add(new Statement(null, null, operand, null));
        }

        private void Emit(TACValue operand, TACValue destination)
        {
            functionStack.Peek().Statements.Add(new Statement(null, null, operand, destination));
        }

        private void Emit(Operator op, TACValue operand, TACValue destination)
        {
            functionStack.Peek().Statements.Add(new Statement(op, null, operand, destination));
        }

        private void Emit(Operator op, TACValue lhs, TACValue rhs, TACValue destination)
        {
            functionStack.Peek().Statements.Add(new Statement(op, lhs, rhs, destination));
        }
        
        private bool IsCaptured(string name, int id)
        {
            return outerSymbolTables.Count > 0 &&
                 outerSymbolTables.Peek().Contains(name) &&
                 outerSymbolTables.Peek().GetSymbol(name).Id == id;
        }

        void AssertEmptyTacValueStack()
        {
            if (tacValueStack.Count != 0)
            {
                throw new InternalCompilerError("TAC value stack is not empty");
            }
        }
    }
}
