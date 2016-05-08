using System;
using CodeGenCourseProject.AST;
using CodeGenCourseProject.ErrorHandling;
using System.Collections.Generic;

namespace CodeGenCourseProject.SemanticChecking
{
    public class SemanticChecker : ASTVisitor
    {
        private ErrorReporter reporter;
        private SymbolTable symbolTable;
        private const string ERROR_TYPE = "errortype";
        public const string INTEGER_TYPE = "integer";
        public const string REAL_TYPE = "real";
        public const string STRING_TYPE = "string";
        public const string BOOLEAN_TYPE = "boolean";

        public const string ARRAY_PREFIX = "Array<";
        public const string ARRAY_SUFFIX = ">";

        public const string INTEGER_ARRAY = ARRAY_PREFIX + INTEGER_TYPE + ARRAY_SUFFIX;
        public const string REAL_ARRAY = ARRAY_PREFIX + REAL_TYPE + ARRAY_SUFFIX;
        public const string STRING_ARRAY = ARRAY_PREFIX + STRING_TYPE + ARRAY_SUFFIX;
        public const string BOOLEAN_ARRAY = ARRAY_PREFIX + BOOLEAN_TYPE + ARRAY_SUFFIX;


        public const string VOID_TYPE = "void";

        private List<string> predeclaredIdentifiers;

        // used to verify return statement type
        private Stack<string> functionReturnTypeStack;

        public SemanticChecker(ErrorReporter reporter)
        {
            this.reporter = reporter;
            symbolTable = new SymbolTable(reporter);
            functionReturnTypeStack = new Stack<string>();

            predeclaredIdentifiers = new List<string>
            { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE, "true", "false", "read", "writeln", "size" };
        }

        public void Visit(ArrayIndexNode node)
        {
            if (node.Children.Count != 2)
            {
                throw new InternalCompilerError("Invalid child count for ArrayIndexNode: " + node.Children.Count);
            }

            node.Children[0].Accept(this);

            var expr = node.Children[1];
            expr.Accept(this);

            if (expr.NodeType() != ERROR_TYPE && expr.NodeType() != INTEGER_TYPE)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Invalid type '" + expr.NodeType() + "' for array indexing",
                    expr.Line,
                    expr.Column);
            }

            var name = (IdentifierNode)node.Children[0];
            var symbol = symbolTable.GetSymbol(name.Value);

            // the caller will handle undeclared identifiers, we merely check that symbol is an array

            if (symbol == null)
            {
                node.SetNodeType(ERROR_TYPE);
                return;
            }

            if (!(symbol is ArraySymbol))
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Cannot index '" + name.Value + "' as it is not an array",
                    node.Line,
                    node.Column);

                ReportPreviousDeclaration(symbol);

                node.SetNodeType(ERROR_TYPE);
                return;
            }

            node.SetNodeType(((ArraySymbol)symbol).BaseType);
        }

        public void Visit(ArrayTypeNode arrayTypeNode)
        {
            if (arrayTypeNode.Children.Count < 1 || arrayTypeNode.Children.Count > 2)
            {
                throw new InternalCompilerError("Invalid child count for ArrayTypeNode: " + arrayTypeNode.Children.Count);
            }

            foreach (var child in arrayTypeNode.Children)
            {
                child.Accept(this);
            }
            if (arrayTypeNode.Children.Count == 2)
            {
                var exprChild = arrayTypeNode.Children[1];
                if (exprChild.NodeType() == ERROR_TYPE)
                {
                    return;
                }

                if (exprChild.NodeType() != INTEGER_TYPE)
                {
                    reporter.ReportError(
                        Error.SEMANTIC_ERROR,
                        "Invalid type '" + exprChild.NodeType() + "' for array size expression",
                        exprChild.Line,
                        exprChild.Column);
                }
            }
        }

        public void Visit(BlockNode blockNode)
        {
            symbolTable.PushLevel();
            foreach (var child in blockNode.Children)
            {
                child.Accept(this);
            }
            blockNode.SetSymbols(symbolTable.PopLevel());
        }

        public void Visit(DivideNode divideNode)
        {
            HandleBinaryOperator(divideNode, "/", new List<string> { INTEGER_TYPE, REAL_TYPE });
        }

        public void Visit(ErrorNode errorNode)
        {
            errorNode.SetNodeType(ERROR_TYPE);
        }

        public void Visit(WhileNode whileNode)
        {
            if (whileNode.Children.Count != 2)
            {
                throw new InternalCompilerError("Invalid child count for WhileNode: " + whileNode.Children.Count);
            }

            foreach (var child in whileNode.Children)
            {
                child.Accept(this);
            }

            var expr = whileNode.Children[0];

            if (expr.NodeType() != ERROR_TYPE && expr.NodeType() != BOOLEAN_TYPE)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Invalid type '" + expr.NodeType() + "' for while statement condition",
                    expr.Line,
                    expr.Column
                    );

                reporter.ReportError(
                    Error.NOTE_GENERIC,
                    "While statement condition must have type 'boolean'", 0, 0);

            }
        }

        public void Visit(ReturnNode returnNode)
        {
            if (returnNode.Children.Count > 1)
            {
                throw new InternalCompilerError("Invalid child count for ReturnNode: " + returnNode.Children.Count);
            }

            if (functionReturnTypeStack.Count == 0)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Return statement outside function or procedure body",
                    returnNode.Line,
                    returnNode.Column);
                return;
            }

            foreach (var child in returnNode.Children)
            {
                child.Accept(this);
            }

            var returnType = functionReturnTypeStack.Peek();

            if (returnNode.Children.Count == 1 && returnType == VOID_TYPE && returnNode.Children[0].NodeType() != ERROR_TYPE)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Return statement in procedure cannot return a value",
                    returnNode.Line,
                    returnNode.Column);
                return;
            }

            if (returnNode.Children.Count == 1 && returnNode.Children[0].NodeType() != returnType &&
               returnNode.Children[0].NodeType() != ERROR_TYPE && returnType != ERROR_TYPE)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Return statement has type '" + returnNode.Children[0].NodeType() +
                    "' when enclosing function has type '" + returnType + "'",
                    returnNode.Children[0].Line,
                    returnNode.Children[0].Column);
                return;
            }

            if (returnNode.Children.Count == 0 && returnType != VOID_TYPE && returnType != ERROR_TYPE)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Return statement must have an expression with type '" + returnType +
                    "', as it is enclosed by a function, not procedure",
                    returnNode.Line,
                    returnNode.Column);
                return;
            }
        }

        public void Visit(SubtractNode subtractNode)
        {
            HandleBinaryOperator(subtractNode, "-", new List<string> { INTEGER_TYPE, REAL_TYPE });
        }

        public void Visit(OrNode orNode)
        {
            HandleBinaryOperator(orNode, "or", new List<string> { BOOLEAN_TYPE });
        }

        public void Visit(MultiplyNode multiplyNode)
        {
            HandleBinaryOperator(multiplyNode, "*", new List<string> { INTEGER_TYPE, REAL_TYPE });
        }

        public void Visit(GreaterThanOrEqualNode greaterThanOrEqualNode)
        {
            HandleBinaryOperator(greaterThanOrEqualNode, ">=", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
            if (greaterThanOrEqualNode.NodeType() != ERROR_TYPE)
            {
                greaterThanOrEqualNode.SetNodeType(BOOLEAN_TYPE);
            }
        }

        public void Visit(RealNode realNode)
        {
            // do nothing
        }

        public void Visit(ProgramNode programNode)
        {
            foreach (var child in programNode.Children)
            {
                child.Accept(this);
            }
        }

        public void Visit(GreaterThanNode greaterThanNode)
        {
            HandleBinaryOperator(greaterThanNode, ">", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
            if (greaterThanNode.NodeType() != ERROR_TYPE)
            {
                greaterThanNode.SetNodeType(BOOLEAN_TYPE);
            }
        }

        public void Visit(IfNode ifNode)
        {
            var children = ifNode.Children.Count;
            if (children < 2 || children > 3)
            {
                throw new InternalCompilerError("Invalid child count for IfNode: " + children);
            }

            foreach (var child in ifNode.Children)
            {
                child.Accept(this);
            }

            var expr = ifNode.Children[0];

            if (expr.NodeType() != ERROR_TYPE && expr.NodeType() != BOOLEAN_TYPE)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Invalid type '" + expr.NodeType() + "' for if statement condition",
                    expr.Line,
                    expr.Column);

                reporter.ReportError(Error.NOTE_GENERIC, "If statement condition must have type 'boolean'", 0, 0);
            }

        }

        public void Visit(IntegerNode integerNode)
        {
            // do nothing
        }

        public void Visit(IdentifierNode identifierNode)
        {
            if (symbolTable.Contains(identifierNode.Value))
            {
                identifierNode.SetNodeType(symbolTable.GetSymbol(identifierNode.Value).Type);
            }
            else if (identifierNode.Value == "true" || identifierNode.Value == "false")
            {
                identifierNode.SetNodeType(BOOLEAN_TYPE);
            }
            else if (!predeclaredIdentifiers.Contains(identifierNode.Value))
            {
                identifierNode.SetNodeType(ERROR_TYPE);
                ReportUndeclaredIdentifier(identifierNode);
            }
            else
            {
                identifierNode.SetNodeType("<predefined identifier>");
            }

        }

        public void Visit(LessThanOrEqualNode lessThanOrEqualNode)
        {
            HandleBinaryOperator(lessThanOrEqualNode, "<=", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
            if (lessThanOrEqualNode.NodeType() != ERROR_TYPE)
            {
                lessThanOrEqualNode.SetNodeType(BOOLEAN_TYPE);
            }
        }

        public void Visit(LessThanNode lessThanNode)
        {
            HandleBinaryOperator(lessThanNode, "<", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
            if (lessThanNode.NodeType() != ERROR_TYPE)
            {
                lessThanNode.SetNodeType(BOOLEAN_TYPE);
            }
        }

        public void Visit(ModuloNode moduloNode)
        {
            HandleBinaryOperator(moduloNode, "%", new List<string> { INTEGER_TYPE });
        }

        public void Visit(UnaryPlusNode unaryPlusNode)
        {
            HandleUnaryOperator(unaryPlusNode, "+", new List<string> { INTEGER_TYPE, REAL_TYPE });
        }

        public void Visit(NegateNode negateNode)
        {
            HandleUnaryOperator(negateNode, "-", new List<string> { INTEGER_TYPE, REAL_TYPE });
        }

        public void Visit(StringNode stringNode)
        {
            // do nothing
        }

        public void Visit(NotNode notNode)
        {
            HandleUnaryOperator(notNode, "not", new List<string> { BOOLEAN_TYPE });
        }

        public void Visit(VariableAssignmentNode variableAssignmentNode)
        {
            if (variableAssignmentNode.Children.Count != 2)
            {
                throw new InternalCompilerError(
                    "Invalid child count for the VariableAssignmentNode: " +
                    variableAssignmentNode.Children.Count);
            }

            var child = variableAssignmentNode.Children[0];

            child.Accept(this);

            IdentifierNode nameNode = null;
            if (child is IdentifierNode)
            {
                nameNode = (IdentifierNode)variableAssignmentNode.Children[0];
            }
            else
            {
                nameNode = (IdentifierNode)variableAssignmentNode.Children[0].Children[0];
            }

            var expression = variableAssignmentNode.Children[1];
            expression.Accept(this);

            var symbol = symbolTable.GetSymbol(nameNode.Value);

            // special case check for predefined identifiers, as these do not get declared as undefined otherwise
            if (symbol == null && predeclaredIdentifiers.Contains(nameNode.Value))
            {
                ReportUndeclaredIdentifier(nameNode);

                return;
            }

            if (symbol is FunctionSymbol)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Cannot assign into function or procedure",
                    nameNode.Line,
                    nameNode.Column);
                ReportPreviousDeclaration(symbol);
                return;
            }

            if (expression.NodeType() != ERROR_TYPE && child.NodeType() != ERROR_TYPE &&
                child.NodeType() != expression.NodeType())
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Cannot assign an expression with type '" + expression.NodeType() + "' into a variable " +
                    "with type '" + child.NodeType() + "'",
                    expression.Line,
                    expression.Column);

                ReportPreviousDeclaration(symbol);
            }

        }

        public void Visit(VariableDeclarationNode variableDeclarationNode)
        {
            var childCount = variableDeclarationNode.Children.Count;
            if (childCount < 2)
            {
                throw new InternalCompilerError(
                    "Invalid child count for the VariableDeclarationNode: " +
                    childCount
                    );
            }
            var typeChild = variableDeclarationNode.Children[0];
            typeChild.Accept(this);

            IdentifierNode varType = null;
            if (typeChild is IdentifierNode)
            {
                varType = (IdentifierNode)typeChild;
            }
            else
            {
                varType = (IdentifierNode)typeChild.Children[0];
            }

            if (symbolTable.Contains(varType.Value))
            {
                ReportUnavailableType(varType);
                return;
            }

            var names = new List<string>();

            for (int i = 1; i < childCount; ++i)
            {
                var nameNode = ((IdentifierNode)variableDeclarationNode.Children[i]);

                if (symbolTable.ContainsOnLevel(nameNode.Value))
                {
                    ReportRedeclaration(nameNode);
                }
                else
                {
                    if (typeChild is ArrayTypeNode)
                    {
                        symbolTable.InsertArray(nameNode.Line, nameNode.Column, nameNode.Value, varType.Value, false);
                    }
                    else if (typeChild is IdentifierNode)
                    {
                        symbolTable.InsertVariable(nameNode.Line, nameNode.Column, nameNode.Value, varType.Value, false);
                    }
                    else
                    {
                        throw new InternalCompilerError("Invalid typeNode: " + typeChild.GetType());
                    }

                }
            }
        }

        public void Visit(NotEqualsNode notEqualsNode)
        {
            HandleBinaryOperator(notEqualsNode, "<>", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
            if (notEqualsNode.NodeType() != ERROR_TYPE)
            {
                notEqualsNode.SetNodeType(BOOLEAN_TYPE);
            }
        }

        public void Visit(ProcedureNode procedureNode)
        {
            var children = procedureNode.Children.Count;
            if (children < 2)
            {
                throw new InternalCompilerError("Invalid child count for ProcedureNode: " + children);
            }

            var name = (IdentifierNode)procedureNode.Children[0];
            var block = procedureNode.Children[1];

            // while block is a BlockNode, and we could just call block.Accept(), 
            // I do not want to open two new symtable levels; this would make it acceptable to shadow
            // function arguments with new declarations. 

            var returnType = VOID_TYPE;
            var paramStartPoint = 2;
            var level = HandleFunctionOrProcedure(procedureNode, name, block, returnType, paramStartPoint);
            ((BlockNode)block).SetSymbols(level);
        }



        public void Visit(FunctionNode functionNode)
        {
            var children = functionNode.Children.Count;
            if (children < 3)
            {
                throw new InternalCompilerError("Invalid child count for FunctionNode: " + children);
            }

            var name = (IdentifierNode)functionNode.Children[0];
            var type = functionNode.Children[1];
            var block = functionNode.Children[2];

            IdentifierNode returnTypeNode = null;
            string returnType = "";
            if (type is IdentifierNode)
            {
                returnTypeNode = (IdentifierNode)type;
                returnType = returnTypeNode.Value;
            }
            else
            {
                returnTypeNode = (IdentifierNode)type.Children[0];
                returnType = ARRAY_PREFIX + returnTypeNode.Value + ARRAY_SUFFIX;
            }

            if (symbolTable.Contains(returnTypeNode.Value))
            {
                ReportUnavailableType(returnTypeNode);
                returnType = ERROR_TYPE;
            }

            var parameterStartPoint = 3;
            var level = HandleFunctionOrProcedure(functionNode, name, block, returnType, parameterStartPoint);
            ((BlockNode)block).SetSymbols(level);
        }

        public void Visit(EqualsNode equalsNode)
        {
            HandleBinaryOperator(equalsNode, "=", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
            if (equalsNode.NodeType() != ERROR_TYPE)
            {
                equalsNode.SetNodeType(BOOLEAN_TYPE);
            }
        }


        public void Visit(CallNode callNode)
        {
            if (callNode.Children.Count < 1)
            {
                throw new InternalCompilerError("Invalid child count for CallNode" + callNode.Children.Count);
            }
            var nameNode = (IdentifierNode)callNode.Children[0];
            var non_arg_children = 1;
            var argumentCount = callNode.Children.Count - non_arg_children;

            var symbol = symbolTable.GetSymbol(nameNode.Value);

            foreach (var child in callNode.Children)
            {
                child.Accept(this);
            }

            callNode.SetNodeType(ERROR_TYPE);
            if (symbol == null)
            {
                // if this is predefined identifier, it is accepted by visit(IdentifierNode)
                // we need to check for the valid use here
                if (predeclaredIdentifiers.Contains(nameNode.Value))
                {
                    var acceptableTypes = new List<string> { ERROR_TYPE, INTEGER_TYPE, BOOLEAN_TYPE, STRING_TYPE, REAL_TYPE };

                    // predefined functions
                    if (nameNode.Value == "read")
                    {
                        if (argumentCount == 0)
                        {
                            reporter.ReportError(
                                Error.SEMANTIC_ERROR,
                                "Predefined procedure 'read' expects at least one argument",
                                nameNode.Line,
                                nameNode.Column);
                            return;
                        }

                        // arguments must be variables, or indexed arrays
                        for (int i = 1; i < argumentCount + 1; ++i)
                        {
                            //    
                            var child = callNode.Children[i];
                            // I'm about 110% sure I will have no idea what this condition does 5 minutes after I'm done with it
                            if (!IsWritable(child, acceptableTypes))
                            {
                                reporter.ReportError(
                                    Error.SEMANTIC_ERROR,
                                    "Invalid argument for predefined function 'read'",
                                    child.Line,
                                    child.Column);

                                reporter.ReportError(
                                    Error.NOTE_GENERIC,
                                    "Argument must be a non-array variable or indexed array",
                                    child.Line,
                                    child.Column);
                            }
                        }
                    }
                    else if (nameNode.Value == "writeln")
                    {
                        for (int i = 1; i < argumentCount + 1; ++i)
                        {

                            var child = callNode.Children[i];
                            // I'm about 110% sure I will have no idea what this condition does 5 minutes after I'm done with it
                            if (!acceptableTypes.Contains(child.NodeType()))
                            {
                                reporter.ReportError(
                                    Error.SEMANTIC_ERROR,
                                    "Invalid argument type '" + child.NodeType() + "' for predefined function 'writeln'",
                                    child.Line,
                                    child.Column);
                            }
                        }
                    }
                    else
                    {
                        ReportUndeclaredIdentifier(nameNode);
                    }
                }


                return;
            }
            else if (!(symbol is FunctionSymbol))
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Identifier '" + symbol.Name + "' is not callable",
                    callNode.Line,
                    callNode.Column
                    );
                ReportPreviousDeclaration(symbol);
                return;
            }

            var functionSymbol = (FunctionSymbol)symbol;

            if (functionSymbol.ParamTypes.Count != argumentCount)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Call to '" + symbol.Name + "' has " + argumentCount + " arguments but '" +
                    symbol.Name + "' has " + functionSymbol.ParamTypes.Count + " parameters",
                    callNode.Line,
                    callNode.Column
                    );
                ReportPreviousDeclaration(symbol);
                return;
            }
            
            int argErrors = 0;
            for (int i = 0; i < argumentCount; ++i)
            {
                var argType = callNode.Children[i + 1].NodeType();
                var paramType = functionSymbol.ParamTypes[i];
                var isReference = functionSymbol.IsReferenceParameters[i];
                if (argType != ERROR_TYPE && paramType != ERROR_TYPE && argType != paramType)
                {
                    reporter.ReportError(
                        Error.SEMANTIC_ERROR,
                        "Argument " + (i + 1) + " for '" + symbol.Name + "' has type '" +
                        argType + "' but corresponding parameter has type '" + paramType + "'",
                        callNode.Children[i + 1].Line,
                        callNode.Children[i + 1].Column);
                    ++argErrors;
                }
                var acceptableTypes = new List<string> {
                    ERROR_TYPE, INTEGER_TYPE, BOOLEAN_TYPE, STRING_TYPE, REAL_TYPE,
                    INTEGER_ARRAY,
                    REAL_ARRAY,
                    STRING_ARRAY,
                    BOOLEAN_ARRAY};

                if (!IsWritable(callNode.Children[i + 1], acceptableTypes) && isReference)
                {
                    reporter.ReportError(
                       Error.SEMANTIC_ERROR,
                       "Argument " + (i + 1) + " for '" + symbol.Name + "' is invalid, " +
                       "as the corresponding parameter is reference type",
                       callNode.Children[i + 1].Line,
                       callNode.Children[i + 1].Column);

                    reporter.ReportError(
                       Error.NOTE_GENERIC,
                       "Argument must be writable",
                       0,0);
                    ++argErrors;
                }
            }

            if (argErrors != 0)
            {
                ReportPreviousDeclaration(symbol);
            }

            callNode.SetNodeType(functionSymbol.BaseType);
        }

        public void Visit(AssertNode assertNode)
        {
            if (assertNode.Children.Count != 1)
            {
                throw new InternalCompilerError("Invalid child count for AssertNode: " + assertNode.Children.Count);
            }

            var expr = assertNode.Children[0];
            expr.Accept(this);

            if (expr.NodeType() != ERROR_TYPE && expr.NodeType() != BOOLEAN_TYPE)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Invalid type '" + expr.NodeType() + "' for assert statement",
                    expr.Line,
                    expr.Column);

                reporter.ReportError(
                    Error.NOTE_GENERIC,
                    "Assert statement must have type 'boolean'", 0, 0);
            }
        }

        public void Visit(ArraySizeNode node)
        {
            if (node.Children.Count != 1)
            {
                throw new InternalCompilerError("Invalid child count for ArraySizeNode: " + node.Children.Count);
            }
            node.Children[0].Accept(this);
            var type = node.Children[0].NodeType();

            if (!type.Contains(ARRAY_PREFIX) && type != ERROR_TYPE)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Cannot get the size of an expression with type '" + type + "'",
                    node.Line,
                    node.Column);

                node.SetNodeType(ERROR_TYPE);
                return;
            }


            node.SetNodeType(INTEGER_TYPE);
        }

        public void Visit(AndNode node)
        {
            HandleBinaryOperator(node, "and", new List<string> { BOOLEAN_TYPE });
        }

        public void Visit(AddNode node)
        {
            HandleBinaryOperator(node, "+", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE });
        }

        public void Visit(FunctionParameterVariableNode functionParameterNode)
        {
            if (functionParameterNode.Children.Count != 2)
            {
                throw new InternalCompilerError("Invalid child count for FunctionParameterVariableNode: " +
                    functionParameterNode.Children.Count);
            }

            if (symbolTable.Contains(functionParameterNode.Type.Value))
            {
                functionParameterNode.SetNodeType(ERROR_TYPE);
                ReportUnavailableType(functionParameterNode.Type);
                return;
            }

            symbolTable.InsertVariable(
                functionParameterNode.Line,
                functionParameterNode.Column,
                functionParameterNode.Name.Value,
                functionParameterNode.Type.Value,
                functionParameterNode.IsReferenceParameter);

            functionParameterNode.SetNodeType(symbolTable.GetSymbol(functionParameterNode.Name.Value).Type);
        }

        public void Visit(FunctionParameterArrayNode functionParameterArrayNode)
        {
            if (functionParameterArrayNode.Children.Count > 3 || functionParameterArrayNode.Children.Count < 2)
            {
                throw new InternalCompilerError("Invalid child count for FunctionParameterArrayNode: " +
                    functionParameterArrayNode.Children.Count);
            }

            functionParameterArrayNode.SetNodeType(ERROR_TYPE);
            if (functionParameterArrayNode.Children.Count == 3)
            {
                var child = functionParameterArrayNode.Children[2];
                child.Accept(this);
                if (child.NodeType() != ERROR_TYPE && child.NodeType() != INTEGER_TYPE)
                {
                    reporter.ReportError(
                        Error.SEMANTIC_ERROR,
                        "Invalid type '" + child.NodeType() + "' for array size expression",
                        child.Line,
                        child.Column);
                }
            }

            if (symbolTable.Contains(functionParameterArrayNode.Type.Value))
            {
                ReportUnavailableType(functionParameterArrayNode.Type);
                return;
            }

            symbolTable.InsertArray(
                functionParameterArrayNode.Line,
                functionParameterArrayNode.Column,
                functionParameterArrayNode.Name.Value,
                functionParameterArrayNode.Type.Value,
                functionParameterArrayNode.IsReferenceParameter);

            functionParameterArrayNode.SetNodeType(symbolTable.GetSymbol(functionParameterArrayNode.Name.Value).Type);
        }

        private void HandleUnaryOperator(ASTNode node, string op, IList<string> acceptableTypes)
        {
            if (node.Children.Count != 1)
            {
                throw new InternalCompilerError(
                    "Invalid child count for unary node '" + op + "': " +
                    node.Children.Count);
            }

            var child = node.Children[0];
            child.Accept(this);


            if (child.NodeType() == ERROR_TYPE)
            {
                node.SetNodeType(ERROR_TYPE);
                return;
            }

            if (!acceptableTypes.Contains(child.NodeType()))
            {
                var types = string.Join(", ", acceptableTypes);
                if (acceptableTypes.Count > 1)
                {
                    types = "one of " + types;
                }
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Invalid type '" + child.NodeType() + "' for operator '" + op + "'",
                    node.Line,
                    node.Column);

                reporter.ReportError(Error.NOTE_GENERIC, "Operator expects " + types, 0, 0);

                node.SetNodeType(ERROR_TYPE);
                return;
            }

            node.SetNodeType(child.NodeType());
        }

        private void HandleBinaryOperator(ASTNode node, string op, IList<string> acceptableTypes)
        {
            if (node.Children.Count != 2)
            {
                throw new InternalCompilerError(
                    "Invalid child count for binary node '" + op + "': " +
                    node.Children.Count);
            }

            var lhs = node.Children[0];
            var rhs = node.Children[1];
            lhs.Accept(this);
            rhs.Accept(this);


            if (lhs.NodeType() == ERROR_TYPE || rhs.NodeType() == ERROR_TYPE)
            {
                node.SetNodeType(ERROR_TYPE);
                return;
            }
            else if (lhs.NodeType() != rhs.NodeType())
            {
                node.SetNodeType(ERROR_TYPE);

                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Invalid types '" + lhs.NodeType() + "' and '" + rhs.NodeType() + "' for operator '" + op + "'",
                    node.Line,
                    node.Column);

                reporter.ReportError(
                    Error.NOTE,
                    "Left hand side expression has type '" + lhs.NodeType() + "'",
                    lhs.Line,
                    lhs.Column);

                reporter.ReportError(
                    Error.NOTE,
                    "Right hand side expression has type '" + rhs.NodeType() + "'",
                    rhs.Line,
                    rhs.Column);

                return;
            }

            if (!acceptableTypes.Contains(lhs.NodeType()))
            {
                var types = string.Join(", ", acceptableTypes);
                if (acceptableTypes.Count > 1)
                {
                    types = "one of " + types;
                }
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Invalid type '" + lhs.NodeType() + "' for operator '" + op + "'",
                    node.Line,
                    node.Column);

                reporter.ReportError(Error.NOTE_GENERIC, "Operator expects " + types, 0, 0);

                node.SetNodeType(ERROR_TYPE);
                return;
            }

            node.SetNodeType(lhs.NodeType());
        }

        private SymbolTableLevel HandleFunctionOrProcedure(ASTNode mainNode, IdentifierNode name, ASTNode block, string returnType, int paramStartPoint)
        {

            var paramTypes = new List<string>();
            var isReference = new List<bool>();
            symbolTable.InsertFunction(
                name.Line,
                name.Column,
                name.Value,
                returnType,
                paramTypes,
                isReference);

            symbolTable.PushLevel();
            functionReturnTypeStack.Push(returnType);
            for (int i = paramStartPoint; i < mainNode.Children.Count; ++i)
            {
                mainNode.Children[i].Accept(this);
                var child = mainNode.Children[i];
                paramTypes.Add(child.NodeType());
                if (child is FunctionParameterArrayNode)
                {
                    isReference.Add(((FunctionParameterArrayNode)child).IsReferenceParameter);
                }
                else if (child is FunctionParameterVariableNode)
                {
                    isReference.Add(((FunctionParameterVariableNode)child).IsReferenceParameter);
                }
                else
                {
                    throw new InternalCompilerError("Invalid child type for function param: " + child.GetType());
                }
            }
            
            foreach (var child in block.Children)
            {
                child.Accept(this);
            }

            var level = symbolTable.PopLevel();
            functionReturnTypeStack.Pop();
            return level;
        }

        private void ReportUnavailableType(IdentifierNode name)
        {
            reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Type '" + name.Value + "' is inaccessible",
                    name.Line,
                    name.Column);

            var symbol = symbolTable.GetSymbol(name.Value);

            reporter.ReportError(
                    Error.NOTE,
                    "Previous declaration overrides the type",
                    symbol.Line,
                    symbol.Column);
        }

        private void ReportRedeclaration(IdentifierNode nameNode)
        {
            reporter.ReportError(
                Error.SEMANTIC_ERROR,
                "Redeclaration of identifier '" + nameNode.Value + "'",
                nameNode.Line,
                nameNode.Column);

            var symbol = symbolTable.GetSymbol(nameNode.Value);
            ReportPreviousDeclaration(symbol);
        }

        private void ReportUndeclaredIdentifier(IdentifierNode nameNode)
        {
            reporter.ReportError(
                Error.SEMANTIC_ERROR,
                "Identifier '" + nameNode.Value + "' has not been declared",
                nameNode.Line,
                nameNode.Column);
        }

        private void ReportPreviousDeclaration(Symbol symbol)
        {
            reporter.ReportError(
                Error.NOTE,
                "Identifier '" + symbol.Name + "' was declared here",
                symbol.Line,
                symbol.Column);
        }

        private bool IsWritable(ASTNode node, IList<string> acceptableTypes)
        {
            if (!(node is IdentifierNode || node is ArrayIndexNode))
            {
                return false;
            }

            if (node is IdentifierNode)
            {
                var ident = (IdentifierNode)node;
                var symbol = symbolTable.GetSymbol(ident.Value); 
                if ((predeclaredIdentifiers.Contains(ident.Value) && symbol == null) ||
                    (symbol != null && !acceptableTypes.Contains(symbol.Type)))
                {
                    return false;
                }
            }

            return true;
        }


    }
}
