﻿using CodeGenCourseProject.AST;
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
            return new ProgramNode(program.Line, program.Column, identifier, block);

        }

        private ASTNode ParseBlock()
        {
            var begin = Expect<BeginToken>();

            try
            {
                var block = new BlockNode(begin.Line, begin.Column);

                while (!(lexer.PeekToken() is EndToken))
                {
                    block.Children.Add(ParseStatement());

                    if (lexer.PeekToken() is SemicolonToken)
                    {
                        lexer.NextToken();
                    }
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

                return new ErrorNode();
            }
        }

        private ASTNode ParseStatement()
        {
            var next = lexer.PeekToken();

            if (next is IdentifierToken)
            {
                return ParseAssignmentOrCallStatement();
            }

            throw new NotImplementedException();
        }

        private ASTNode ParseAssignmentOrCallStatement()
        {
            var identifier = Expect<IdentifierToken>();

            var next = lexer.PeekToken();
            return ParseAssignmentStatement();
            
            throw new NotImplementedException();
        }

        private ASTNode ParseAssignmentStatement()
        {
            // backtrack once, so that we can use the ParseVariable() - method
            lexer.Backtrack();
            var variable = ParseVariable();

            var assignmentToken = Expect<AssignmentToken>();
            var expression = ParseExpression();

            return new VariableAssignmentNode(
                assignmentToken.Line,
                assignmentToken.Column,
                variable,
                expression);
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
                    throw new NotImplementedException();
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


            throw new NotImplementedException();
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
            var token = lexer.NextToken();

            if (token is T)
            {
                return (T)token;
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
    }
}
