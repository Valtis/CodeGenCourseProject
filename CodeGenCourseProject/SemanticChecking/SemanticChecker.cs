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

        public SemanticChecker(ErrorReporter reporter)
        {
            this.reporter = reporter;
            table = new SymbolTable();
        }

        public void Visit(ArrayIndexNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(ArrayTypeNode arrayTypeNode)
        {
            throw new NotImplementedException();
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
            HandleBinaryNode(divideNode, "/", new List<string> { INTEGER_TYPE, REAL_TYPE });
        }

        public void Visit(ErrorNode errorNode)
        {
            errorNode.SetNodeType(ERROR_TYPE);
        }

        public void Visit(WhileNode whileNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(ReturnNode returnNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(SubtractNode subtractNode)
        {
            HandleBinaryNode(subtractNode, "-", new List<string> { INTEGER_TYPE, REAL_TYPE });
        }

        public void Visit(OrNode orNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(MultiplyNode multiplyNode)
        {
            HandleBinaryNode(multiplyNode, "*", new List<string> { INTEGER_TYPE, REAL_TYPE });
        }

        public void Visit(ProcedureNode procedureNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(GreaterThanOrEqualNode greaterThanOrEqualNode)
        {
            HandleBinaryNode(greaterThanOrEqualNode, ">=", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
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
            HandleBinaryNode(greaterThanNode, ">", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
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
        }

        public void Visit(LessThanOrEqualNode lessThanOrEqualNode)
        {
            HandleBinaryNode(lessThanOrEqualNode, "<=", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
            if (lessThanOrEqualNode.NodeType() != ERROR_TYPE)
            {
                lessThanOrEqualNode.SetNodeType(BOOLEAN_TYPE);
            }
        }

        public void Visit(LessThanNode lessThanNode)
        {
            HandleBinaryNode(lessThanNode, "<", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
            if (lessThanNode.NodeType() != ERROR_TYPE)
            {
                lessThanNode.SetNodeType(BOOLEAN_TYPE);
            }
        }

        public void Visit(ModuloNode moduloNode)
        {
            HandleBinaryNode(moduloNode, "%", new List<string> { INTEGER_TYPE });
        }

        public void Visit(UnaryPlusNode unaryPlusNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(NegateNode negateNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(StringNode stringNode)
        {
            // do nothing
        }

        public void Visit(NotNode notNode)
        {
            throw new NotImplementedException();
        }

        public void Visit(VariableAssignmentNode variableAssignmentNode)
        {
            if (variableAssignmentNode.Children.Count != 2)
            {
                throw new InternalCompilerError(
                    "Invalid child count for the VariableAssingmentNode: " + 
                    variableAssignmentNode.Children.Count);
            }
            var nameNode = (IdentifierNode)variableAssignmentNode.Children[0];



            var expression = variableAssignmentNode.Children[1];
            expression.Accept(this);


            var symbol = table.GetSymbol(nameNode.Value);

            if (symbol == null)
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Variable '" + nameNode.Value + "' has not been declared",
                    nameNode.Line,
                    nameNode.Column);
            }
            else
            {
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

            var type = (IdentifierNode)variableDeclarationNode.Children[0];

            if (table.Contains(type.Value))
            {
                reporter.ReportError(
                    Error.SEMANTIC_ERROR,
                    "Type '" + type.Value + "' is inaccessible",
                    type.Line, 
                    type.Column);

                var symbol = table.GetSymbol(type.Value);

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
                    table.InsertSymbol(nameNode.Line, nameNode.Column, nameNode.Value, type.Value);
                }
                    

            }
            
            foreach (var name in names)
            {
                
            }
            
        }

        private void ReportPreviousDeclaration(Symbol symbol)
        {
            reporter.ReportError(
                Error.NOTE,
                "Declared here",
                symbol.Line,
                symbol.Column);
        }

        public void Visit(NotEqualsNode notEqualsNode)
        {
            HandleBinaryNode(notEqualsNode, "<>", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
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
            HandleBinaryNode(equalsNode, "=", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE, BOOLEAN_TYPE });
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
            throw new NotImplementedException();
        }

        public void Visit(AndNode node)
        {
            throw new NotImplementedException();
        }

        public void Visit(AddNode node)
        {
            HandleBinaryNode(node, "+", new List<string> { INTEGER_TYPE, REAL_TYPE, STRING_TYPE });
        }


        private void HandleBinaryNode(ASTNode node, string op, IList<string> acceptableTypes)
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


    }
}
