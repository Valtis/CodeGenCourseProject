using CodeGenCourseProject.AST;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC.Values;
using System;
using System.Collections.Generic;
using static CodeGenCourseProject.TAC.Function;

namespace CodeGenCourseProject.TAC
{
    public enum Operator
    {
        PLUS, MINUS, MULTIPLY, DIVIDE, MODULO, CONCAT,
        LESS_THAN, LESS_THAN_OR_EQUAL, EQUAL, GREATER_THAN_OR_EQUAL, GREATER_THAN, NOT_EQUAL, AND, OR, NOT
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
                default:
                    throw new InternalCompilerError("Default branch taken in Operator.Name()");
            }
        }
    }

    public class TACGenerator : ASTVisitor
    {
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
        private Stack<ISet<Parameter>> capturedIdentifiers;
        // contains outer symbol tables (symbol table without current function locals etc.)
        // used to identify captured variables
        private Stack<SymbolTable> outerSymbolTables;

        public TACGenerator()
        {
            functions = new List<Function>();
            functionStack = new Stack<Function>();
            tacValueStack = new Stack<TACValue>();
            symbolTable = new SymbolTable(null);

            outerSymbolTables = new Stack<SymbolTable>();
            capturedIdentifiers = new Stack<ISet<Parameter>>();
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

        public void Visit(ArrayIndexNode node)
        {
            var nameNode = (IdentifierNode)node.Children[0];
            nameNode.Accept(this);
            tacValueStack.Pop();
            var name = nameNode.Value;
            var expr = node.Children[1];

            expr.Accept(this);

            var symbol = (ArraySymbol)symbolTable.GetSymbol(name);

            var outerSymbol =
               outerSymbolTables.Count > 0 ?
                   outerSymbolTables.Peek().GetSymbol(name, node.Line)
                   : null;

            var exprTAC = tacValueStack.Pop();
            var index = new TACArrayIndex(node.Line, node.Column,
                    nameNode.Value, exprTAC, symbol.BaseType, symbol.Id, symbol.IsReference);
            
            tacValueStack.Push(index);
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

        public void Visit(CallNode callNode)
        {
            var name = ((IdentifierNode)callNode.Children[0]).Value;
            var arguments = new List<TACValue>();
            var functionSymbol = (FunctionSymbol)symbolTable.GetSymbol(name, callNode.Line);

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
          
            // inbuilt writeln function
            if (name == "writeln" && functionSymbol == null)
            {
                Emit(new TACCallWriteln(callNode.Line, callNode.Column, arguments));
                return;
            }
            // inbuilt read function
            if (name == "read" && functionSymbol == null)
            {
                Emit(new TACCallRead(callNode.Line, callNode.Column, arguments));
                return;
            }

            var call = new TACCall(
                    callNode.Line,
                    callNode.Column,
                    Helper.MangleFunctionName(name, functionSymbol.Id),
                    arguments);

            if (callNode.NodeType() == SemanticChecker.VOID_TYPE)
            {
                Emit(call);
            }
            else
            {
                var temp = GetTemporary(callNode);
                Emit(call, temp);
                tacValueStack.Push(temp);
            }
        }

        private TACValue GenerateCopyIfNeeded(CallNode callNode, FunctionSymbol functionSymbol, int i, TACValue argument)
        {
            var argumentSymbol = symbolTable.GetSymbol(
                                    ((TACIdentifier)argument).UnmangledName, callNode.Line);
            if (!(argumentSymbol is ArraySymbol))
            {
                return argument;
            }
            var functionArgumentIsReference = functionSymbol.IsReferenceParameters[i - 1];

            if (functionArgumentIsReference)
            {
                return argument;
            }
            var temporary = GetTemporary(callNode.Children[i]);
            Emit(new TACCloneArray((TACIdentifier)argument, temporary));

            argument = temporary;
            return argument;
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
                Emit(new TACReturn(returnNode.Line, returnNode.Column));
                return;
            }
            returnNode.Children[0].Accept(this);
            Emit(new TACReturn(returnNode.Line, returnNode.Column, tacValueStack.Pop()));
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
            var symbol = symbolTable.GetSymbol(name, identifierNode.Line);

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
                    outerSymbolTables.Peek().GetSymbol(name, identifierNode.Line)
                    : null;


            // if symbol is the same symbol than one in the outer symbol table 
            // (table without current function declarations), the variable is captured
            // from outer context
            TACIdentifier identifier;
            if (symbol == outerSymbol)
            {
                var isReference = false;
                if (outerSymbol is VariableSymbol)
                {
                    isReference = ((VariableSymbol)outerSymbol).IsReference;
                }
                else if (outerSymbol is ArraySymbol)
                {
                    isReference = ((ArraySymbol)outerSymbol).IsReference;
                }


                identifier = new TACIdentifier(
                    identifierNode.Line, identifierNode.Column, name, symbol.Type, symbol.Id, isReference);
                capturedIdentifiers.Peek().Add(
                    new Parameter(
                        identifier,
                        symbol.Type,
                        true)); // always pass as reference
            }
            else
            {
                var isReference = false;
                if (symbol is VariableSymbol)
                {
                    isReference = ((VariableSymbol)symbol).IsReference;
                }
                else if (outerSymbol is ArraySymbol)
                {
                    isReference = ((ArraySymbol)symbol).IsReference;
                }

                identifier = new TACIdentifier(
                    identifierNode.Line, identifierNode.Column, name, symbol.Type, symbol.Id, isReference);
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

            TACLabel endIfBlock = null;
            TACLabel endElseBlock = null;

            if (hasElseBlock)
            {
                endIfBlock = GetLabel(ifNode.Children[2].Line, ifNode.Children[2].Column);
                endElseBlock = GetLabel(0, 0);
            }
            else
            {
                endIfBlock = GetLabel(0, 0);
            }

            expression.Accept(this);
            var condition = tacValueStack.Pop();


            Emit(new TACJumpIfFalse(ifBlock.Line, ifBlock.Column, condition, endIfBlock));
            ifBlock.Accept(this);

            if (hasElseBlock)
            {
                Emit(new TACJump(ifBlock.Line, ifBlock.Column, endElseBlock));
            }

            Emit(endIfBlock);

            if (hasElseBlock)
            {
                var elseBlock = ifNode.Children[2];
                elseBlock.Accept(this);
                Emit(endElseBlock);
            }
            AssertEmptyTacValueStack();
        }

        public void Visit(GreaterThanNode greaterThanNode)
        {
            HandleBinaryOperator(greaterThanNode, Operator.GREATER_THAN);
        }

        public void Visit(ProgramNode programNode)
        {
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
                type = ((IdentifierNode)((ArrayTypeNode)functionNode.Children[1]).Children[0]).Value;
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

            if (variableDeclarationNode.Children[0] is ArrayTypeNode)
            {
                variableDeclarationNode.Children[0].Accept(this);
                var arraySize = tacValueStack.Pop();
                for (int i = 1; i < variableDeclarationNode.Children.Count; ++i)
                {
                    var name = (IdentifierNode)variableDeclarationNode.Children[i];
                    var symbol = (ArraySymbol)symbolTable.GetSymbol(name.Value);
                    Emit(new TACArrayDeclaration(
                        variableDeclarationNode.Line, variableDeclarationNode.Column, name.Value, symbol.BaseType, arraySize, symbol.Id));
                    AssertEmptyTacValueStack();
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
            var beginLabel = GetLabel(whileNode.Children[1].Line, whileNode.Children[1].Column);


            var endLabel = GetLabel(0, 0);
            Emit(beginLabel);

            whileNode.Children[0].Accept(this);
            var condition = tacValueStack.Pop();
            Emit(new TACJumpIfFalse(whileNode.Line, whileNode.Column, condition, endLabel));
            whileNode.Children[1].Accept(this);


            Emit(new TACJump(whileNode.Line, whileNode.Column, beginLabel));
            Emit(endLabel);

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

            Emit(new TACAssert(assertNode.Line, assertNode.Column, tacValueStack.Pop()));

            AssertEmptyTacValueStack();
        }

        public void Visit(ArraySizeNode node)
        {
            node.Children[0].Accept(this);
            tacValueStack.Push(
                new TACArraySize(node.Line, node.Column, tacValueStack.Pop()));
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
            Emit(op, rhs, tacValue);
            tacValueStack.Push(tacValue);
        }

        private TACIdentifier GetTemporary(ASTNode node)
        {
            var curId = tempID++;
            return new TACIdentifier(node.Line, node.Column, "__t", node.NodeType(), curId);
        }

        private TACLabel GetLabel(int line, int column)
        {
            return new TACLabel(line, column, labelID++);
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
            capturedIdentifiers.Push(new HashSet<Parameter>());
            functionStack.Push(function);

            // honestly starting to regret how I decided to handle function\procedure nodes in AST.
            // Too many dirty workarounds.
            // this one in particular is because I need the symbol table info here
            // that is only present in the child block

            node.Children[block].Accept(this);
            symbolTable.PushLevel(((BlockNode)node.Children[block]).GetSymbols());
            for (int i = paramStart; i < node.Children.Count; ++i)
            {
                node.Children[i].Accept(this);
            }

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
            functionStack.Peek().Statements.Add(new TACStatement(null, null, operand, null));
        }

        private void Emit(TACValue operand, TACValue destination)
        {
            functionStack.Peek().Statements.Add(new TACStatement(null, null, operand, destination));
        }

        private void Emit(Operator op, TACValue operand, TACValue destination)
        {
            functionStack.Peek().Statements.Add(new TACStatement(op, null, operand, destination));
        }

        private void Emit(Operator op, TACValue lhs, TACValue rhs, TACValue destination)
        {
            functionStack.Peek().Statements.Add(new TACStatement(op, lhs, rhs, destination));
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
