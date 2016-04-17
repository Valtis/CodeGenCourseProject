using CodeGenCourseProject.AST;
using CodeGenCourseProject.ErrorHandling;
using CodeGenCourseProject.Lexing;
using CodeGenCourseProject.Tokens;
using System;
using System.Collections.Generic;

namespace CodeGenCourseProject.Parsing
{
    public class Parser
    {
        private Lexer lexer;
        private ErrorReporter reporter;

        private IDictionary<Type, Type> relationalOperators;
        private IDictionary<Type, Type> additionOperators;
        private IDictionary<Type, Type> multiplyOperators;

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

            return ParseAssignmentStatement();

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
            nodes.Add(ParseExpression());

            while (lexer.PeekToken() is CommaToken)
            {
                Expect<CommaToken>();
                nodes.Add(ParseExpression());
            }

            return nodes;
        }

        private ASTNode ParseAssignmentStatement()
        {
            // backtrack once, so that we can use the ParseVariable() - method
            lexer.Backtrack();
            var variable = ParseVariable();
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
                if (lexer.PeekToken() is SemicolonToken || lexer.PeekToken() is EndToken)
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
                Expect<ThenToken>();
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
                Expect<DoToken>();
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
            Token negate = null;
            if (lexer.PeekToken() is MinusToken)
            {
                negate = Expect<MinusToken>();
            }

            // discard plus sign
            if (lexer.PeekToken() is PlusToken)
            {
                Expect<PlusToken>();
            }

            var term = ParseTerm();

            if (negate != null)
            {
                if (term is IntegerNode)
                {
                    term = new IntegerNode(term.Line, term.Column, -((IntegerNode)term).Value);
                }
                else if (term is RealNode)
                {
                    term = new RealNode(term.Line, term.Column, -((RealNode)term).Value);
                }
                else
                {
                    term = new NegateNode(negate.Line, negate.Column, term);
                }
            }

            if (additionOperators.ContainsKey(lexer.PeekToken().GetType()))
            {
                var op = lexer.NextToken();
                var term2 = ParseTerm();
                var binaryNode = (ASTNode)Activator.CreateInstance(
                    additionOperators[op.GetType()],
                    op.Line,
                    op.Column,
                    term,
                    term2
                    );
                return binaryNode;
            }

            return term;
        }


        private ASTNode ParseTerm()
        {
            var factor = ParseFactor();

            if (multiplyOperators.ContainsKey(lexer.PeekToken().GetType()))
            {
                var op = lexer.NextToken();
                var factor2 = ParseFactor();
                var binaryNode = (ASTNode)Activator.CreateInstance(
                    multiplyOperators[op.GetType()],
                    op.Line,
                    op.Column,
                    factor,
                    factor2
                    );
                return binaryNode;
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
                return ParseVariable();
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

        private ASTNode ParseVariable()
        {
            var identifier = Expect<IdentifierToken>();

            if (lexer.PeekToken() is LBracketToken)
            {
                Expect<LBracketToken>();

                var expr = ParseExpression();

                Expect<RBracketToken>();

                return new ArrayIndexNode(identifier.Line, identifier.Column, identifier, expr);
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
            SkipTo(new List<Type> { typeof(EndToken), typeof(SemicolonToken) });
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
