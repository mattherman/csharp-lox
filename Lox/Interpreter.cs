using System;
using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private readonly Environment _globals = new Environment();
        private Environment _environment;
        private Dictionary<Expr, int> _locals = new Dictionary<Expr, int>();

        public Interpreter()
        {
            _globals.Define("clock", new Clock()); 
            _environment = _globals;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            var left = Evaluate(expr.Left);

            if (expr.Op.Type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.Right);
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            var obj = Evaluate(expr.Obj);

            if (obj is LoxInstance instance)
            {
                var val = Evaluate(expr.Value);
                instance.Set(expr.Name, val);
                return val;
            }

            throw new RuntimeException(expr.Name, "Only instances have fields.");
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expr);
        }

        public object VisitUnaryExpr(Expr.Unary expr)
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

        private object LookUpVariable(Token name, Expr expr)
        {
            var found = _locals.TryGetValue(expr, out var depth);
            if (found)
            {
                return _environment.GetAt(depth, name);
            }
            return _globals.Get(name);
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.Name, expr);
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            var val = Evaluate(expr.Value);
            var found = _locals.TryGetValue(expr, out var depth);
            if (found)
            {
                _environment.AssignAt(depth, expr.Name, val);
            }
            else
            {
                _globals.Assign(expr.Name, val);
            }
            return val;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Op.Type)
            {
                case TokenType.EQUAL_EQUAL:
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
                        return (double)left + (double)right;
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

        public object VisitCallExpr(Expr.Call expr)
        {
            var callee = Evaluate(expr.Callee);
            var arguments = expr.Arguments.Select(Evaluate).ToList();

            var function = callee as ILoxCallable;
            if (function == null)
            {
                throw new RuntimeException(expr.Paren, "Can only call functions and classes.");
            }
            if (arguments.Count != function.Arity)
            {
                throw new RuntimeException(expr.Paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.Expr);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return null;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            var previousEnvironment = _environment;
            try
            {
                _environment = environment;
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previousEnvironment;
            }
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            var result = Evaluate(stmt.Expr);
            Console.WriteLine(Stringify(result));
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            var returnValue = stmt.Value != null ? Evaluate(stmt.Value) : null;
            throw new Return(returnValue);
        }
 
        public object VisitVarStmt(Stmt.Var stmt)
        {
            object val = null;
            if (stmt.Initializer != null)
            {
                val = Evaluate(stmt.Initializer);
            }

            _environment.Define(stmt.Name, val);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new LoxFunction(stmt, _environment);
            _environment.Define(stmt.Name.Lexeme, function);
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            // Two step define/assign so that members of the class can
            // reference the class itself
            _environment.Define(stmt.Name.Lexeme, null);

            var methods = new Dictionary<string, LoxFunction>();
            foreach (var method in stmt.Methods)
            {
                var function = new LoxFunction(method, _environment);
                methods[method.Name.Lexeme] = function;
            }

            var klass = new LoxClass(stmt.Name.Lexeme, methods);
            _environment.Assign(stmt.Name, klass);
            return null;
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

        private void Execute(Stmt statement)
        {
            statement.Accept(this);
        }

        public void Resolve(Expr expr, int depth)
        {
            _locals.Add(expr, depth);
        }

        public void Interpret(IList<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
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

        public object VisitGetExpr(Expr.Get expr)
        {
            var obj = Evaluate(expr.Obj);
            if (obj is LoxInstance instance)
            {
                return instance.Get(expr.Name);
            }

            throw new RuntimeException(expr.Name, "Only instances have properties.");
        }
    }
}
