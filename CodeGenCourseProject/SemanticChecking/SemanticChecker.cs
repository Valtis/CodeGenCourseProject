using System;
using CodeGenCourseProject.AST;
using CodeGenCourseProject.ErrorHandling;
using System.Collections.Generic;

namespace CodeGenCourseProject.SemanticChecking
{
    public class SemanticChecker : ASTVisitor
    {
        private ErrorReporter reporter;
        private SymbolTable table;
        private const string ERROR_TYPE = "errortype";
        private const string INTEGER_TYPE = "integer";
        private const string REAL_TYPE = "real";
        private const string STRING_TYPE = "string";
        private const string BOOLEAN_TYPE = "boolean";

        private List<string> predeclaredIdentifiers;

        public SemanticChecker(ErrorReporter reporter)
        {
            this.reporter = reporter;
            table = new SymbolTable();

            predeclaredIdentifiers = new List<string>
            { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE };
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
            var symbol = table.GetSymbol(name.Value);

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
                    "Cannot index '" + name.Value +"' as it is not an array",
                    node.Line,
                    node.Column);

                ReportPreviousDeclaration(symbol);

                node.SetNodeType(ERROR_TYPE);
                return;
            }

            node.SetNodeType(symbol.Type);

        }

        public void Visit(ArrayTypeNode arrayTypeNode)
        {
            if (arrayTypeNode.Children.Count != 2)
            {
                throw new InternalCompilerError("Invalid child count for ArrayTypeNode: " + arrayTypeNode.Children.Count);
            } 

            foreach (var child in arrayTypeNode.Children)
            {
                child.Accept(this);
            }

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

        public void Visit(BlockNode blockNode)
        {
            table.PushLevel();
            foreach (var child in blockNode.Children)
            {
                child.Accept(this);
            }
            table.PopLevel();
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


            if (expr.NodeType() != ERROR_TYPE & expr.NodeType() != BOOLEAN_TYPE)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Invalid type '" + expr.NodeType() +"' for while statement condition",
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
            throw new NotImplementedException();
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

        public void Visit(ProcedureNode procedureNode)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Visit(IntegerNode integerNode)
        {
            // do nothing
        }

        public void Visit(IdentifierNode identifierNode)
        {
            if (table.Contains(identifierNode.Value))
            {
                identifierNode.SetNodeType(table.GetSymbol(identifierNode.Value).Type);
            }
            else if (identifierNode.Value == "true" || identifierNode.Value == "false")
            {
                identifierNode.SetNodeType(BOOLEAN_TYPE);
            }
            else if (!predeclaredIdentifiers.Contains(identifierNode.Value))
            {
                identifierNode.SetNodeType(ERROR_TYPE);
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Identifier '" + identifierNode.Value + "' has not been declared",
                    identifierNode.Line,
                    identifierNode.Column);
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
            
            var symbol = table.GetSymbol(nameNode.Value);

            
            if (symbol == null)
            {
                // special case check - true/valid are not reported by Visit(IdentifierNode), but treated as a keyword, 
                // if no such variable is declared -> we must check if the variable name is true/false, and check if they are
                // declared


                if (nameNode.Value == "true" || nameNode.Value == "false")
                {
                    reporter.ReportError(
                        Error.SEMANTIC_ERROR,
                        "Identifier '" + nameNode.Value + "' has not been declared",
                        nameNode.Line,
                        nameNode.Column);
                }
                return;
            }
            else
            {
                // check that if variable we are assigning to is actually a variable, and not something else

                if (child is IdentifierNode && !(symbol is VariableSymbol))
                {
                    reporter.ReportError(
                        Error.SEMANTIC_ERROR,
                        "Cannot assign into '" + nameNode.Value + "' as it is not a regular variable",
                        child.Line,
                        child.Column);


                    ReportPreviousDeclaration(symbol);
                    return;
                }
                
                if (expression.NodeType() != ERROR_TYPE && symbol.Type != expression.NodeType())
                {
                    reporter.ReportError(
                        Error.SEMANTIC_ERROR,
                        "Cannot assign an expression with type '" + expression.NodeType() + "' into a variable " +
                        "with type '" + symbol.Type + "'", 
                        expression.Line,
                        expression.Column);

                    ReportPreviousDeclaration(symbol);
                }
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

            IdentifierNode name = null;
            if (typeChild is IdentifierNode)
            {
                name = (IdentifierNode)typeChild;
            }
            else
            {
                name = (IdentifierNode)typeChild.Children[0];
            }

            if (table.Contains(name.Value))
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Type '" + name.Value + "' is inaccessible",
                    name.Line, 
                    name.Column);

                var symbol = table.GetSymbol(name.Value);

                reporter.ReportError(
                        Error.NOTE,
                        "Previous declaration overrides the type",
                        symbol.Line,
                        symbol.Column);
                return;
            }

            var names = new List<string>();

            for (int i = 1; i < childCount; ++i)
            {
                var nameNode = ((IdentifierNode)variableDeclarationNode.Children[i]);

                if (table.ContainsOnLevel(nameNode.Value))
                {
                    reporter.ReportError(
                        Error.SEMANTIC_ERROR,
                        "Redeclaration of identifier '" + nameNode.Value + "'",
                        nameNode.Line,
                        nameNode.Column);

                    var symbol = table.GetSymbol(nameNode.Value);
                    ReportPreviousDeclaration(symbol);
                }
                else
                {
                    if (typeChild is ArrayTypeNode)
                    {
                        table.InsertArray(nameNode.Line, nameNode.Column, nameNode.Value, name.Value);
                    }
                    else if (typeChild is IdentifierNode)
                    {
                        table.InsertVariable(nameNode.Line, nameNode.Column, nameNode.Value, name.Value);
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

        public void Visit(FunctionNode functionNode)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Visit(AssertNode assertNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(ArraySizeNode node)
        {
            if (node.Children.Count != 1)
            {
                throw new InternalCompilerError("Invalid child count for ArraySizeNode: " + node.Children.Count);
            }
            node.Children[0].Accept(this);

            var array = (IdentifierNode)node.Children[0];
            var symbol = table.GetSymbol(array.Value);
            if (symbol != null && !(symbol is ArraySymbol))
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Cannot get the size of an non-array object '" + array.Value + "'",
                    node.Line,
                    node.Column);
                
                ReportPreviousDeclaration(symbol);
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
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Invalid types '" + lhs.NodeType() + "' and '" + rhs.NodeType() + "' for operator '" + op + "'",
                    node.Line,
                    node.Column);
                node.SetNodeType(ERROR_TYPE);

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

        private void ReportPreviousDeclaration(Symbol symbol)
        {
            reporter.ReportError(
                Error.NOTE,
                "Identifier was previously declared here",
                symbol.Line,
                symbol.Column);
        }

    }
}
