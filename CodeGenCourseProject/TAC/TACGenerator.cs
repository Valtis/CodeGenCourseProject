using CodeGenCourseProject.AST;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC.Values;
using System;
using System.Collections.Generic;

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
                    return ">=";
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

        public TACGenerator()
        {
            functions = new List<Function>();
            functionStack = new Stack<Function>();
            tacValueStack = new Stack<TACValue>();
            symbolTable = new SymbolTable(null);
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
            var expr = node.Children[1];

            expr.Accept(this);

            var symbol = (ArraySymbol)symbolTable.GetSymbol(nameNode.Value);

            var exprTAC = tacValueStack.Pop();
            tacValueStack.Push(
                new TACArrayIndex(node.Line, node.Column,
                    nameNode.Value, exprTAC, symbol.BaseType, symbol.Id));
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
            }

            symbolTable.PopLevel();
        }

        public void Visit(CallNode callNode)
        {
            AssertEmptyTacValueStack();
            var name = ((IdentifierNode)callNode.Children[0]).Value;

            var symbol = symbolTable.GetSymbol(name);
            // inbuilt writeln function
            if (name == "writeln" && symbol == null)
            {
                for (int i = 1; i < callNode.Children.Count; ++i)
                {
                    callNode.Children[i].Accept(this);
                }

                var list = new List<TACValue>();
                while (tacValueStack.Count != 0)
                {
                    list.Add(tacValueStack.Pop());
                }
                list.Reverse();
                Emit(new TACCallWriteln(callNode.Line, callNode.Column, list));
                AssertEmptyTacValueStack();
                return;
            }

            if (name == "read" && symbol == null)
            {
                for (int i = 1; i < callNode.Children.Count; ++i)
                {
                    callNode.Children[i].Accept(this);
                }

                var list = new List<TACValue>();
                while (tacValueStack.Count != 0)
                {
                    list.Add(tacValueStack.Pop());
                }
                list.Reverse();
                Emit(new TACCallRead(callNode.Line, callNode.Column, list));
                AssertEmptyTacValueStack();
                return;
            }

            AssertEmptyTacValueStack();
            throw new NotImplementedException();
        }

        public void Visit(EqualsNode equalsNode)
        {
            HandleBinaryOperator(equalsNode, Operator.EQUAL);
        }

        public void Visit(FunctionNode functionNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(FunctionParameterVariableNode functionParameterNode)
        {
            // for name mangling. FIXME: Extract name mangling code into separate helper
            
            var symbol = symbolTable.GetSymbol(functionParameterNode.Name.Value);
            var id = new TACIdentifier(symbol.Name, symbol.Type, symbol.Id);

            functionStack.Peek().
                AddParameter(
                    id,
                    functionParameterNode.IsReferenceParameter);
        }

        public void Visit(ReturnNode returnNode)
        {
            if (returnNode.Children.Count == 0)
            {
                Emit(new TACReturn(returnNode.Line, returnNode.Column));
                return;
            }

            throw new NotImplementedException();
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
            var symbol = symbolTable.GetSymbol(name);

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
            tacValueStack.Push(
                new TACIdentifier(identifierNode.Line, identifierNode.Column, name, symbol.Type, symbol.Id));
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
            var function = new Function(ENTRY_POINT);
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
            var function = new Function("__"+((IdentifierNode)procedureNode.Children[0]).Value);
            functionStack.Push(function);

            // honestly starting to regret how I decided to handle function\procedure nodes in AST.
            // Too many dirty workarounds.
            // this one in particular is because I need the symbol table info here
            // that is only present in the child block

            procedureNode.Children[1].Accept(this);

            symbolTable.PushLevel(((BlockNode)procedureNode.Children[1]).GetSymbols());
            for (int i = 2; i < procedureNode.Children.Count; ++i)
            {
                procedureNode.Children[i].Accept(this);
            }
            symbolTable.PopLevel();
            Functions.Add(functionStack.Pop());
        }

        public void Visit(MultiplyNode multiplyNode)
        {
            HandleBinaryOperator(multiplyNode, Operator.MULTIPLY);
        }

        public void Visit(FunctionParameterArrayNode functionParameterArrayNode)
        {
            // for name mangling. FIXME: Extract name mangling code into separate helper

            var symbol = symbolTable.GetSymbol(functionParameterArrayNode.Name.Value);
            var id = new TACIdentifier(symbol.Name, symbol.Type, symbol.Id);

            functionStack.Peek().
                AddParameter(
                    id,
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
            throw new NotImplementedException();
        }

        public void Visit(AssertNode assertNode)
        {
            throw new NotImplementedException();
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
