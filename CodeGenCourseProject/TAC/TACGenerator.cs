using CodeGenCourseProject.AST;
using CodeGenCourseProject.SemanticChecking;
using CodeGenCourseProject.TAC.Values;
using System;
using System.Collections.Generic;

namespace CodeGenCourseProject.TAC
{
    public enum Operator { PLUS, MINUS, MULTIPLY, DIVIDE, MODULO };
    
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
        private int id;
        private SymbolTable symbolTable;
        private Stack<TACValue> tacValueStack; // used to store temporaries when evaluating sub-expressions

        public TACGenerator()
        {
            functions = new List<Function>();
            functionStack = new Stack<Function>();
            tacValueStack = new Stack<TACValue>();
            symbolTable = new SymbolTable(null);
            id = 0;
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
                new TACArrayIndex(nameNode.Value, exprTAC, symbol.BaseType, symbol.Id));            
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
                arraySize = new TACInteger(0);
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
                Emit(new TACCallWriteln(list));
                AssertEmptyTacValueStack();
                return;                   
            }

            AssertEmptyTacValueStack();
            throw new NotImplementedException();
        }

        public void Visit(EqualsNode equalsNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(FunctionNode functionNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(FunctionParameterVariableNode functionParameterNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(ReturnNode returnNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(SubtractNode subtractNode)
        {
            HandleBinaryOperator(subtractNode, Operator.MINUS);
        }

        public void Visit(OrNode orNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(NotNode notNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(StringNode stringNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(NegateNode negateNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(ModuloNode moduloNode)
        {
            HandleBinaryOperator(moduloNode, Operator.MODULO);
        }

        public void Visit(LessThanNode lessThanNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(LessThanOrEqualNode lessThanOrEqualNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(IdentifierNode identifierNode)
        {
            var name = identifierNode.Value;
            var symbol = symbolTable.GetSymbol(name);
            if (symbol == null)
            {
                return; // identifier is predeclared identifier - skip
            }
            tacValueStack.Push(
                new TACIdentifier(name, symbol.Type, symbol.Id));
        }

        public void Visit(IntegerNode integerNode)
        {
            tacValueStack.Push(new TACInteger(integerNode.Value));
        }

        public void Visit(IfNode ifNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(GreaterThanNode greaterThanNode)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Visit(GreaterThanOrEqualNode greaterThanOrEqualNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(ProcedureNode procedureNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(MultiplyNode multiplyNode)
        {
            HandleBinaryOperator(multiplyNode, Operator.MULTIPLY);
        }

        public void Visit(FunctionParameterArrayNode functionParameterArrayNode)
        {
            throw new NotImplementedException();
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
                    Emit(new TACArrayDeclaration(name.Value, symbol.BaseType, arraySize, symbol.Id));
                    AssertEmptyTacValueStack();
                }
            }

        }

        public void Visit(NotEqualsNode notEqualsNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(WhileNode whileNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(ErrorNode errorNode)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Visit(AndNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(AddNode node)
        {
            HandleBinaryOperator(node, Operator.PLUS);
        }

        private void HandleBinaryOperator(ASTNode node, Operator op)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this);
            }

            var curId = id++;
            var tacValue = new TACIdentifier("__t", node.NodeType(), curId);

            var rhs = tacValueStack.Pop();
            var lhs = tacValueStack.Pop();
            Emit(op, lhs, rhs, tacValue);
            
            tacValueStack.Push(tacValue);
        }

        private void Emit(TACValue operand)
        {
            functionStack.Peek().Code.Add(new TACStatement(null, null, operand, null));
        }

        private void Emit(TACValue operand, TACValue destination)
        {
            functionStack.Peek().Code.Add(new TACStatement(null, null, operand, destination));
        }

        private void Emit(Operator op, TACValue operand, TACValue destination)
        {
            functionStack.Peek().Code.Add(new TACStatement(op, null, operand, destination));
        }

        private void Emit(Operator op, TACValue lhs, TACValue rhs, TACValue destination)
        {
            functionStack.Peek().Code.Add(new TACStatement(op, lhs, rhs, destination));
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
