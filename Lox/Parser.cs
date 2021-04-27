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
                if (Match(TokenType.FUN)) return Function("function");
                if (Match(TokenType.VAR)) return VarDeclaration();
                return Statement();
            }
            catch (ParseException)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt Function(string kind)
        {
            var name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
            var parameters = new List<Token>();

            Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }
                    parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
                } while (Match(TokenType.COMMA));
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

            // Consume the '{' before calling Block since it assumes the brace token has already been matched
            Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body.");
            var body = Block();

            return new Stmt.Function(name, parameters, body);
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
            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (Match(TokenType.VAR))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'for' clauses.");

            var body = Statement();

            // De-sugaring the for loop into a while loop
            //
            // Example:
            // for (var a = 0; a < 10; a = a + 1) { <body>; }
            // becomes..
            // var a = 0;
            // while (a < 10) { <body>; a = a + 1; }

            // If there is an increment, create a new block with the increment appended to the loop body
            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt> { body, new Stmt.Expression(increment) });
            }

            // If there was no condition, default it to 'true'
            condition ??= new Expr.Literal(true);

            // Transform body into a while loop with condition
            body = new Stmt.While(condition, body);

            // If there was an initializer, create a new block with the initializer and generated while loop
            if (initializer != null)
            {
                body = new Stmt.Block(new List<Stmt> { initializer, body });
            }

            return body;
        }

        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            var expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'if' condition.");

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
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            var expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after 'while' condition.");

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

            return Call();
        }

        private Expr Call()
        {
            var expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            var paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
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
