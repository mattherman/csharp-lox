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

        public IList<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }
            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.VAR)) return VarDeclaration();
                return Statement();
            }
            catch (ParseException)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt VarDeclaration()
        {
            var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration");
            return new Stmt.Var(name, initializer);
        }

        private Stmt Statement()
        {
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after if.");
            var expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            var thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(expr, thenBranch, elseBranch);
        }

        private Stmt PrintStatement()
        {
            var expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(expr);
        }

        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after while.");
            var expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");

            var body = Statement();

            return new Stmt.While(expr, body);
        }

        private List<Stmt> Block()
        {
            var statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr Assignment()
        {
            var expr = Or();

            if (Match(TokenType.EQUAL))
            {
                var equal = Previous();
                var val = Assignment();

                if (expr is Expr.Variable varExpr)
                {
                    return new Expr.Assign(varExpr.Name, val);
                }

                Error(equal, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr LogicalExpression(Func<Expr> nextExpression, TokenType tokenToMatch)
        {
            var expr = nextExpression();

            while (Match(tokenToMatch))
            {
                var op = Previous();
                var right = nextExpression();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr Or()
        {
            return LogicalExpression(And, TokenType.OR);
        }

        private Expr And()
        {
            return LogicalExpression(Equality, TokenType.AND);
        }

        private Expr BinaryExpression(Func<Expr> nextExpression, params TokenType[] tokensToMatch)
        {
            var expr = nextExpression();

            while (Match(tokensToMatch))
            {
                var op = Previous();
                var right = nextExpression();
                expr = new Expr.Binary(expr, op, right);
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
                return new Expr.Unary(op, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.IDENTIFIER))
            {
                return new Expr.Variable(Previous());
            }

            if (Match(TokenType.LEFT_PAREN))
            {
                var expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
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
