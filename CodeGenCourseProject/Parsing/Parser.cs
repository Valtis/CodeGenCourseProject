using CodeGenCourseProject.AST;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.Tokens;
using System;
using System.Collections.Generic;

namespace CodeGenCourseProject.Parsing
{
    /*
    Recursive descent parser.
    Nothing too odd going on in here. Follows the language grammar and builds AST.
    Syntax errors are reported.
    */
    public class Parser
    {
        private Lexer lexer;
        private ErrorReporter reporter;

        /* Token - AstNode pairs for operators */
        private IDictionary<Type, Type> relationalOperators;
        private IDictionary<Type, Type> additionOperators;
        private IDictionary<Type, Type> multiplyOperators;

        // valid types (integer, real, string, boolean)
        private ISet<string> validTypes;

        public Parser(Lexer lexer, ErrorReporter reporter)
        {
            this.lexer = lexer;
            this.reporter = reporter;

            multiplyOperators = new Dictionary<Type, Type>();
            multiplyOperators.Add(typeof(MultiplyToken), typeof(MultiplyNode));
            multiplyOperators.Add(typeof(DivideToken), typeof(DivideNode));
            multiplyOperators.Add(typeof(ModuloToken), typeof(ModuloNode));
            multiplyOperators.Add(typeof(AndToken), typeof(AndNode));

            additionOperators = new Dictionary<Type, Type>();
            additionOperators.Add(typeof(PlusToken), typeof(AddNode));
            additionOperators.Add(typeof(MinusToken), typeof(SubtractNode));
            additionOperators.Add(typeof(OrToken), typeof(OrNode));

            relationalOperators = new Dictionary<Type, Type>();
            relationalOperators.Add(typeof(LessThanToken), typeof(LessThanNode));
            relationalOperators.Add(typeof(LessThanOrEqualToken), typeof(LessThanOrEqualNode));
            relationalOperators.Add(typeof(EqualsToken), typeof(EqualsNode));
            relationalOperators.Add(typeof(GreaterThanOrEqualToken), typeof(GreaterThanOrEqualNode));
            relationalOperators.Add(typeof(GreaterThanToken), typeof(GreaterThanNode));
            relationalOperators.Add(typeof(NotEqualsToken), typeof(NotEqualsNode));

            // sorted set instead of hash set for more consistent error reporting
            validTypes = new SortedSet<string>();
            validTypes.Add("integer");
            validTypes.Add("real");
            validTypes.Add("boolean");
            validTypes.Add("string");
        }

        public ASTNode Parse()
        {
            return ParseProgram();
        }

        private ASTNode ParseProgram()
        {
            Token program;
            IdentifierToken identifier;
            ASTNode block = null;
            try
            {
                program = Expect<ProgramToken>();
                identifier = Expect<IdentifierToken>();
                Expect<SemicolonToken>();
                block = ParseBlock();
            }
            catch (InvalidParseException ex)
            {
                return new ErrorNode();
            }

            try
            {
                Expect<PeriodToken>();
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                    Error.NOTE_GENERIC,
                    "Main block must end in a '.'",
                    0,
                    0);
            }

            if (!(lexer.PeekToken() is EOFToken))
            {
                reporter.ReportError(
                    Error.SYNTAX_ERROR,
                    "Unexpected token " + lexer.PeekToken().ToString() + " after the end of program block",
                    lexer.PeekToken().Line,
                    lexer.PeekToken().Column);
            }

            return new ProgramNode(program.Line, program.Column, identifier, block);

        }

        private ASTNode ParseBlock()
        {
            var begin = Expect<BeginToken>();

            try
            {
                var block = new BlockNode(begin.Line, begin.Column);


                // initial statement in the block
                block.Children.Add(ParseStatement());

                while (!(lexer.PeekToken() is EndToken) && !(lexer.PeekToken() is EOFToken))
                {
                    try
                    {
                        Expect<SemicolonToken>();
                    }
                    catch
                    {
                        block.Children.Add(new ErrorNode());
                        SyncAfterError();
                        continue;
                    }

                    if (lexer.PeekToken() is EndToken || lexer.PeekToken() is EOFToken)
                    {
                        break;
                    }

                    block.Children.Add(ParseStatement());
                }


                Expect<EndToken>();

                return block;
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                    Error.NOTE,
                    "Error occured when parsing code block beginning here",
                    begin.Line,
                    begin.Column);
                SyncAfterError();

                return new ErrorNode();
            }
        }

        private ASTNode ParseStatement()
        {
            var next = lexer.PeekToken();
            try
            {
                if (next is IdentifierToken)
                {
                    return ParseAssignmentOrCallStatement();
                }
                else if (next is ReturnToken)
                {
                    return ParseReturnStatement();
                }
                else if (next is AssertToken)
                {
                    return ParseAssertStatement();
                }
                else if (next is BeginToken)
                {
                    return ParseBlock();
                }
                else if (next is IfToken)
                {
                    return ParseIfStatement();
                }
                else if (next is WhileToken)
                {
                    return ParseWhileStatement();
                }
                else if (next is VarToken)
                {
                    return ParseVariableDeclarationStatement();
                }
                else if (next is ProcedureToken)
                {
                    return ParseProcedureDeclaration();
                }
                else if (next is FunctionToken)
                {
                    return ParseFunctionDeclaration();
                }
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                   Error.NOTE,
                   "Error occured when parsing statement",
                   next.Line,
                   next.Column);
                SyncAfterError();
                return new ErrorNode();
            }

            reporter.ReportError(
                Error.SYNTAX_ERROR,
                "Unexpected token " + next.ToString() + " when start of a statement was expected",
                 next.Line,
                 next.Column);
            SyncAfterError();
            return new ErrorNode();
        }

        private ASTNode ParseAssignmentOrCallStatement()
        {
            var identifier = Expect<IdentifierToken>();

            var next = lexer.PeekToken();
            if (next is LParenToken)
            {
                return ParseCall(identifier);
            }

            return ParseAssignmentStatement(identifier);

        }

        private ASTNode ParseCall(IdentifierToken identifier)
        {
            try
            {
                Expect<LParenToken>();
                var arguments = new List<ASTNode>();
                if (!(lexer.PeekToken() is RParenToken))
                {
                    arguments = ParseArguments();
                }

                Expect<RParenToken>();

                return new CallNode(
                    identifier.Line,
                    identifier.Column,
                    new IdentifierNode(identifier.Line, identifier.Column, identifier.Value),
                    arguments.ToArray()
                    );
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                   Error.NOTE,
                   "Error occured when parsing function call",
                   identifier.Line,
                   identifier.Column);
                SyncAfterError();
                return new ErrorNode();
            }
        }

        private List<ASTNode> ParseArguments()
        {

            var nodes = new List<ASTNode>();

            try
            {
                nodes.Add(ParseExpression());

                while (lexer.PeekToken() is CommaToken)
                {
                    Expect<CommaToken>();
                    nodes.Add(ParseExpression());
                }
            }
            catch (InvalidParseException ex)
            {
                SkipTo(new List<Type>
                {
                    typeof(EndToken),
                    typeof(SemicolonToken),
                    typeof(RParenToken)
                });
                nodes.Add(new ErrorNode());
                return nodes;
            }

            return nodes;
        }

        private ASTNode ParseAssignmentStatement(IdentifierToken identifier)
        {
            var variable = ParseVariable(identifier);
            Token assignmentToken;
            try
            {
                assignmentToken = Expect<AssignmentToken>();
            }
            catch (InvalidParseException ex)
            {
                var token = lexer.PeekToken();
                if (token is EqualsToken)
                {
                    reporter.ReportError(Error.NOTE_GENERIC,
                        "Perhaps you meant to use assignment ':=' instead of comparison '='?",
                        0,
                        0);
                }
                throw;
            }


            ASTNode expression = null;
            try
            {
                expression = ParseExpression();
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                    Error.NOTE,
                    "Error occured while parsing variable assignment",
                    assignmentToken.Line,
                    assignmentToken.Column);
                SyncAfterError();
                return new ErrorNode();
            }

            return new VariableAssignmentNode(
                assignmentToken.Line,
                assignmentToken.Column,
                variable,
                expression);
        }

        private ASTNode ParseReturnStatement()
        {
            var ret = Expect<ReturnToken>();
            try
            {
                if (
                    lexer.PeekToken() is SemicolonToken || 
                    lexer.PeekToken() is EndToken || 
                    lexer.PeekToken() is ElseToken)
                {
                    return new ReturnNode(ret.Line, ret.Column);
                }

                return new ReturnNode(ret.Line, ret.Column, ParseExpression());
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                    Error.NOTE,
                    "Error occured while parsing return statement",
                    ret.Line,
                    ret.Column);
                SyncAfterError();
                return new ErrorNode();
            }
        }

        private ASTNode ParseAssertStatement()
        {
            var assert = Expect<AssertToken>();
            return new AssertNode(assert.Line, assert.Column, ParseExpression());
        }

        private ASTNode ParseIfStatement()
        {
            var ifToken = Expect<IfToken>();
            try
            {
                var expression = ParseExpression();

                // special case handling for better error messages
                if (lexer.PeekToken() is BeginToken)
                {
                    reporter.ReportError(
                        Error.SYNTAX_ERROR,
                        "If statement condition must be followed by 'then'",
                        lexer.PeekToken().Line,
                        lexer.PeekToken().Column);

                    reporter.ReportError(
                        Error.NOTE,
                        "Error occured while parsing if statement",
                        ifToken.Line,
                        ifToken.Column);
                }
                else
                {
                    Expect<ThenToken>();
                }
                var body = ParseStatement();

                if (!(body is BlockNode))
                {
                    body = new BlockNode(body.Line, body.Column, body);
                }

                if (lexer.PeekToken() is ElseToken)
                {
                    Expect<ElseToken>();
                    var elseBody = ParseStatement();
                    if (!(elseBody is BlockNode))
                    {
                        elseBody = new BlockNode(elseBody.Line, elseBody.Column, elseBody);
                    }

                    return new IfNode(ifToken.Line, ifToken.Column, expression, body, elseBody);
                }

                return new IfNode(ifToken.Line, ifToken.Column, expression, body);
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                    Error.NOTE,
                    "Error occured while parsing if statement",
                    ifToken.Line,
                    ifToken.Column);
                SyncAfterError();
                return new ErrorNode();
            }
        }

        private ASTNode ParseWhileStatement()
        {
            var whileToken = Expect<WhileToken>();
            try
            {
                var expression = ParseExpression();

                // special casing for better error messages
                if (lexer.PeekToken() is BeginToken)
                {
                    reporter.ReportError(
                        Error.SYNTAX_ERROR,
                        "While statement condition must be followed by 'do'",
                        lexer.PeekToken().Line,
                        lexer.PeekToken().Column);

                    reporter.ReportError(
                        Error.NOTE,
                        "Error occured while parsing while statement",
                        whileToken.Line,
                        whileToken.Column);
                }
                else
                { 
                    Expect<DoToken>();
                }
                var body = ParseStatement();

                if (!(body is BlockNode))
                {
                    body = new BlockNode(body.Line, body.Column, body);
                }

                return new WhileNode(whileToken.Line, whileToken.Column, expression, body);
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                    Error.NOTE,
                    "Error occured while parsing while statement",
                    whileToken.Line,
                    whileToken.Column);
                SyncAfterError();
                return new ErrorNode();
            }
        }

        private ASTNode ParseVariableDeclarationStatement()
        {
            var varToken = Expect<VarToken>();
            try
            {
                return ParseVariableDeclaration(varToken);
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                    Error.NOTE,
                    "Error occured while parsing variable declaration",
                    varToken.Line,
                    varToken.Column);
                SyncAfterError();
                return new ErrorNode();
            }
        }

        private ASTNode ParseVariableDeclaration(Token token)
        {

            var identifiers = new List<ASTNode>();
            var identifier = Expect<IdentifierToken>();
            identifiers.Add(new IdentifierNode(identifier.Line, identifier.Column, identifier.Value));

            while (lexer.PeekToken() is CommaToken)
            {
                Expect<CommaToken>();
                identifier = Expect<IdentifierToken>();
                identifiers.Add(new IdentifierNode(identifier.Line, identifier.Column, identifier.Value));
            }

            Expect<ColonToken>();

            var typeNode = ParseType();

            return new VariableDeclarationNode(token.Line, token.Column, typeNode, identifiers.ToArray());
        }

        private ASTNode ParseType()
        {
            IdentifierToken identifier;
            ASTNode typeNode;
            if (lexer.PeekToken() is ArrayToken)
            {
                var array = Expect<ArrayToken>();
                Expect<LBracketToken>();
                ASTNode expression = null;
                if (!(lexer.PeekToken() is RBracketToken))
                {
                    expression = ParseExpression();
                }

                Expect<RBracketToken>();
                Expect<OfToken>();
                identifier = Expect<IdentifierToken>();
                ValidateType(identifier);

                typeNode = new ArrayTypeNode(
                    array.Line,
                    array.Column,
                    new IdentifierNode(identifier.Line, identifier.Column, identifier.Value),
                    expression);
            }
            else
            {
                var typeToken = Expect<IdentifierToken>();
                ValidateType(typeToken);
                typeNode = new IdentifierNode(typeToken.Line, typeToken.Column, typeToken.Value);
            }

            return typeNode;
        }

        private void ValidateType(IdentifierToken type)
        {
            if (!validTypes.Contains(type.Value))
            {
                reporter.ReportError(
                    Error.SYNTAX_ERROR,
                    "'" + type.Value + "' is not a valid type",
                    type.Line,
                    type.Column);

                var types = string.Join(", ", validTypes);
                reporter.ReportError(
                    Error.NOTE_GENERIC,
                    "Valid types are " + types,
                    0, 0);
                throw new InvalidParseException();
            }
        }

        private ASTNode ParseProcedureDeclaration()
        {
            var procedure = Expect<ProcedureToken>();
            try
            {
                var identifier = Expect<IdentifierToken>();
                Expect<LParenToken>();
                var arguments = ParseParameters();
                Expect<RParenToken>();

                // for better error handling
                if (lexer.PeekToken() is BeginToken)
                {
                    reporter.ReportError(Error.SYNTAX_ERROR,
                        "Procedure declaration must end in ';'",
                        lexer.PeekToken().Line,
                        lexer.PeekToken().Column);

                    reporter.ReportError(Error.NOTE,
                        "Error occured while parsing procedure declaration",
                        procedure.Line,
                        procedure.Column);
                }
                else
                {
                    Expect<SemicolonToken>();
                }

                var block = ParseBlock();
                return new ProcedureNode(procedure.Line, procedure.Column,
                    new IdentifierNode(identifier.Line, identifier.Column, identifier.Value),
                    block,
                    arguments.ToArray()
                    );
            } catch (InvalidParseException ex)
            {
                reporter.ReportError(
                   Error.NOTE,
                   "Error occured while parsing procedure declaration",
                   procedure.Line,
                   procedure.Column);
                SyncAfterError();
                return new ErrorNode();
            }
        }

        private ASTNode ParseFunctionDeclaration()
        {
            var function = Expect<FunctionToken>();
            try
            {
                var identifier = Expect<IdentifierToken>();
                Expect<LParenToken>();
                var arguments = ParseParameters();
                Expect<RParenToken>();
                Expect<ColonToken>();
                var type = ParseType();

                // for better error handling
                if (lexer.PeekToken() is BeginToken)
                {
                    reporter.ReportError(Error.SYNTAX_ERROR,
                        "Procedure declaration must end in ';'",
                        lexer.PeekToken().Line,
                        lexer.PeekToken().Column);

                    reporter.ReportError(Error.NOTE,
                        "Error occured while parsing function declaration",
                        function.Line,
                        function.Column);
                }
                else
                {
                    Expect<SemicolonToken>();
                }

                var block = ParseBlock();
                return new FunctionNode(function.Line, function.Column,
                    new IdentifierNode(identifier.Line, identifier.Column, identifier.Value),
                    type,
                    block,
                    arguments.ToArray()
                    );
            }
            catch (InvalidParseException ex)
            {
                reporter.ReportError(
                   Error.NOTE,
                   "Error occured while parsing function declaration",
                   function.Line,
                   function.Column);
                SyncAfterError();
                return new ErrorNode();
            }
        }

        private List<ASTNode> ParseParameters()
        {
            var nodes = new List<ASTNode>();
            if (lexer.PeekToken() is RParenToken)
            {
                return nodes;
            }

            nodes.Add(ParseFunctionParameter());
            while (lexer.PeekToken() is CommaToken)
            {
                Expect<CommaToken>();
                nodes.Add(ParseFunctionParameter());                
            }

            return nodes;
        }

        private ASTNode ParseFunctionParameter()
        {
            bool isReference = false;
            if (lexer.PeekToken() is VarToken)
            {
                isReference = true;
                lexer.NextToken();
            }

            var name = Expect<IdentifierToken>();
            Expect <ColonToken>();
            var type = ParseType();
            
            if (type is IdentifierNode)
            {

                return new FunctionParameterVariableNode(
                    name.Line,
                    name.Column,
                    new IdentifierNode(name.Line, name.Column, name.Value),
                    (IdentifierNode)type,
                    isReference);
            }
            else
            {
                ArrayTypeNode arType = (ArrayTypeNode)type;
                return new FunctionParameterArrayNode(
                    name.Line,
                    name.Column,
                    arType.Children.Count > 1 ? arType.Children[1] : null,
                    new IdentifierNode(name.Line, name.Column, name.Value),
                    (IdentifierNode)arType.Children[0],
                    isReference);
            }
        }            


        private ASTNode ParseExpression()
        {
            var expr = ParseSimpleExpression();

            if (relationalOperators.ContainsKey(lexer.PeekToken().GetType()))
            {
                var op = lexer.NextToken();
                var expr2 = ParseSimpleExpression();
                var binaryNode = (ASTNode)Activator.CreateInstance(
                    relationalOperators[op.GetType()],
                    op.Line,
                    op.Column,
                    expr,
                    expr2
                    );
                return binaryNode;
            }

            return expr;
        }

        private ASTNode ParseSimpleExpression()
        {
            Type sign = null;
            Token signToken = null;
            if (lexer.PeekToken() is MinusToken)
            {
                signToken = Expect<MinusToken>();
                sign = typeof(NegateNode);
            } else if (lexer.PeekToken() is PlusToken)
            {
                signToken = Expect<PlusToken>();
                sign = typeof(UnaryPlusNode);                
            }

            var term = ParseTerm();


            // if we have a sign:
            // for integers and reals, just apply it directly (plus is no-op)
            // for other nodes, add UnaryPlusNode or NegateNode. Semantic checker will check the validity
            // of both, although otherwise UnaryPlusNode is no-op
            if (sign != null)
            {
                if (term is IntegerNode)
                {
                    if (sign == typeof(NegateNode))
                    {
                        term = new IntegerNode(term.Line, term.Column, -((IntegerNode)term).Value);
                    }
                }
                else if (term is RealNode)
                {
                    if (sign == typeof(NegateNode))
                    {
                        term = new RealNode(term.Line, term.Column, -((RealNode)term).Value);
                    }
                }
                else
                {
                    term = (ASTNode)Activator.CreateInstance(sign, signToken.Line, signToken.Column, term);
                }
            }
            
            while (additionOperators.ContainsKey(lexer.PeekToken().GetType()))
            {
                var op = lexer.NextToken();
                var term2 = ParseTerm();
                term = (ASTNode)Activator.CreateInstance(
                    additionOperators[op.GetType()],
                    op.Line,
                    op.Column,
                    term,
                    term2
                    );
                
            }

            return term;
        }


        private ASTNode ParseTerm()
        {
            var factor = ParseFactor();

            while (multiplyOperators.ContainsKey(lexer.PeekToken().GetType()))
            {
                var op = lexer.NextToken();
                var factor2 = ParseFactor();
                factor = (ASTNode)Activator.CreateInstance(
                    multiplyOperators[op.GetType()],
                    op.Line,
                    op.Column,
                    factor,
                    factor2
                    );
            }

            return factor;
        }

        private ASTNode ParseFactor()
        {
            var subFactor = ParseSubFactor();

            if (lexer.PeekToken() is PeriodToken)
            {
                Expect<PeriodToken>();
                var identifier = Expect<IdentifierToken>();

                if (identifier.Value != "size")
                {
                    ReportUnexpectedToken(new IdentifierToken("size"), identifier);
                }
                subFactor = new ArraySizeNode(subFactor.Line, subFactor.Column, subFactor);
            }

            return subFactor;
        }

        private ASTNode ParseSubFactor()
        {
            var token = lexer.PeekToken();

            if (token is IntegerToken)
            {
                var integer = Expect<IntegerToken>();
                return new IntegerNode(integer.Line, integer.Column, integer.Value);
            }
            else if (token is RealToken)
            {
                var real = Expect<RealToken>();
                return new RealNode(real.Line, real.Column, real.Value);
            }
            else if (token is StringToken)
            {
                var str = Expect<StringToken>();
                return new StringNode(str.Line, str.Column, str.Value);
            }
            else if (token is IdentifierToken)
            {
                return ParseVariableOrCall();
            }
            else if (token is NotToken)
            {
                var not = Expect<NotToken>();
                return new NotNode(not.Line, not.Column, ParseFactor());
            }
            else if (token is LParenToken)
            {
                Expect<LParenToken>();
                var expr = ParseExpression();
                Expect<RParenToken>();
                return expr;
            }

            reporter.ReportError(
                Error.SYNTAX_ERROR,
                "Unexpected token " + token.ToString() + " when expression was expected",
                token.Line,
                token.Column);

            throw new InvalidParseException();
        }

        private ASTNode ParseVariableOrCall()
        {
            var identifier = Expect<IdentifierToken>();
            if (lexer.PeekToken() is LParenToken)
            {
                return ParseCall(identifier);
            }
            else
            {
                return ParseVariable(identifier);
            }
        }

        private ASTNode ParseVariable(IdentifierToken identifier)
        {
            if (lexer.PeekToken() is LBracketToken)
            {
                Expect<LBracketToken>();

                var expr = ParseExpression();

                Expect<RBracketToken>();

                return new ArrayIndexNode(identifier.Line, identifier.Column, new IdentifierNode(identifier.Line, identifier.Column, identifier.Value), expr);
            }

            return new IdentifierNode(identifier.Line, identifier.Column, identifier.Value);
        }

        private T Expect<T>() where T : Token, new()
        {
            var token = lexer.PeekToken();

            if (token is T)
            {
                return (T)lexer.NextToken();
            }
            else
            {
                ReportUnexpectedToken(new T(), token);
                throw new InvalidParseException();
            }
        }

        private void ReportUnexpectedToken(Token expected, Token actual)
        {
            reporter.ReportError(
                Error.SYNTAX_ERROR,
                "Expected token " + expected + " but was actually " + actual,
                actual.Line,
                actual.Column);
            throw new InvalidParseException();
        }

        private void SyncAfterError()
        {
            SkipTo(new List<Type> {
                typeof(EndToken),
                typeof(SemicolonToken) });
        }

        private void SkipTo(IList<Type> types)
        {
            while (true)
            {
                var token = lexer.PeekToken();
                if (token is EOFToken || types.Contains(token.GetType()))
                {
                    break;
                }

                lexer.NextToken();
            }
        }
    }
}
