namespace CodeGenCourseProject.AST
{
    public interface ASTVisitor
    {
        void Visit(AddNode node);
        void Visit(AndNode node);
        void Visit(ArrayIndexNode node);
        void Visit(ArraySizeNode node);
        void Visit(ArrayTypeNode arrayTypeNode);
        void Visit(AssertNode assertNode);
        void Visit(BlockNode blockNode);
        void Visit(UnaryPlusNode unaryPlusNode);
        void Visit(CallNode callNode);
        void Visit(DivideNode divideNode);
        void Visit(EqualsNode equalsNode);
        void Visit(ErrorNode errorNode);
        void Visit(FunctionNode functionNode);
        void Visit(WhileNode whileNode);
        void Visit(FunctionParameterVariableNode functionParameterNode);
        void Visit(NotEqualsNode notEqualsNode);
        void Visit(ReturnNode returnNode);
        void Visit(VariableDeclarationNode variableDeclarationNode);
        void Visit(SubtractNode subtractNode);
        void Visit(VariableAssignmentNode variableAssignmentNode);
        void Visit(OrNode orNode);
        void Visit(FunctionParameterArrayNode functionParameterArrayNode);
        void Visit(NotNode notNode);
        void Visit(MultiplyNode multiplyNode);
        void Visit(StringNode stringNode);
        void Visit(ProcedureNode procedureNode);
        void Visit(NegateNode negateNode);
        void Visit(GreaterThanOrEqualNode greaterThanOrEqualNode);
        void Visit(ModuloNode moduloNode);
        void Visit(RealNode realNode);
        void Visit(LessThanNode lessThanNode);
        void Visit(ProgramNode programNode);
        void Visit(LessThanOrEqualNode lessThanOrEqualNode);
        void Visit(GreaterThanNode greaterThanNode);
        void Visit(IdentifierNode identifierNode);
        void Visit(IfNode ifNode);
        void Visit(IntegerNode integerNode);
    }
}
