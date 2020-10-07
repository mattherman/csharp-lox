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
                    CheckNumberOperand(expr.Op, right);
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
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left <= (double)right;
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left * (double)right;
                case TokenType.PLUS:
                    if (left is Double && right is Double)
                    {
                        return (double)left + (double) right;
                    }
                    if (left is String && right is String)
                    {
                        return (String)left + (String)right;
                    }
                    throw new RuntimeException(expr.Op, "Operands must be two numbers or two strings.");
                default:
                    return null;
            }
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is Double) return;
            throw new RuntimeException(op, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token op, object leftOperand, object rightOperand)
        {
            if (leftOperand is Double && rightOperand is Double) return;
            throw new RuntimeException(op, "Operands must be numbers.");
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

        public void Interpret(Expr expr)
        {
            try
            {
                var result = Evaluate(expr);
                Console.WriteLine(Stringify(result));
            }
            catch (RuntimeException ex)
            {
                Lox.RuntimeError(ex);
            }
        }

        private string Stringify(object value)
        {
            if (value == null) return "nil";
            return value.ToString();
        }
    }
}
