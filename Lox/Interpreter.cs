using System;

namespace Lox
{
    public class Interpreter : IVisitor<object>
    {
        public object VisitLiteralExpr(Literal expr)
        {
            return expr.Value;
        }

        public object VisitGroupingExpr(Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitUnaryExpr(Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    return -(double)right;
                default:
                    return null;
            }
        }

        public object VisitBinaryExpr(Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.EQUAL:
                    return IsEqual(left, right);
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.GREATER:
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    return (double)left <= (double)right;
                case TokenType.MINUS:
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    return (double)left / (double)right;
                case TokenType.STAR:
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left is Double && right is Double)
                    {
                        return (double)left + (double) right;
                    }
                    else if (left is String && right is String)
                    {
                        return (String)left + (String)right;
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;
            return a.Equals(b);
        }

        private bool IsTruthy(object value)
        {
            switch (value)
            {
                case null:
                    return false;
                case Boolean b:
                    return b;
                default:
                    return true;
            }
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }
    }
}
