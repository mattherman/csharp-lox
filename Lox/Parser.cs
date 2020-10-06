using System;
using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Parser
    {
        private readonly IList<Token> _tokens;
        private int _current = 0;

        private bool IsAtEnd() => Peek().Type == TokenType.EOF;
        private Token Peek() => _tokens[_current];
        private Token Previous() => _tokens[_current - 1];

        public Parser(IList<Token> tokens)
        {
            _tokens = tokens;
        }

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseException ex)
            {
                return null;
            }
        }

        private Expr Expression()
        {
            return Equality();
        }

        private Expr BinaryExpression(Func<Expr> nextExpression, params TokenType[] tokensToMatch)
        {
            var expr = nextExpression();

            while (Match(tokensToMatch))
            {
                var op = Previous();
                var right = nextExpression();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            return BinaryExpression(Comparison, TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL);
        }

        private Expr Comparison()
        {
            return BinaryExpression(Addition, TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL);
        }

        private Expr Addition()
        {
            return BinaryExpression(Multiplication, TokenType.MINUS, TokenType.PLUS);
        }

        private Expr Multiplication()
        {
            return BinaryExpression(Unary, TokenType.SLASH, TokenType.STAR);
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                var op = Previous();
                var right = Unary();
                return new Unary(op, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Literal(false);
            if (Match(TokenType.TRUE)) return new Literal(true);
            if (Match(TokenType.NIL)) return new Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Literal(Previous().Literal);
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Check(type)) return Advance();
            
            throw Error(Peek(), errorMessage);
        }

        private bool Match(params TokenType[] types)
        {
            if (types.Any(type => Check(type)))
            {
                Advance();
                return true;
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }    

                Advance();
            }
        }

        private ParseException Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseException();
        }

        private class ParseException : Exception {}
    }
}
