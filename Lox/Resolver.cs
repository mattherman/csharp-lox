using System;
using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private enum FunctionType
        {
            None,
            Function,
            Initializer,
            Method
        }

        private enum ClassType
        {
            None,
            Class,
            Subclass
        }

        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType _currentFunction = FunctionType.None;
        private ClassType _currentClass = ClassType.None;

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }

        private void Resolve(Expr expression)
        {
            expression.Accept(this);
        }

        private void Resolve(Stmt statement)
        {
            statement.Accept(this);
        }

        public void Resolve(IList<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (var i = 0; i < _scopes.Count; i++)
            {
                if (_scopes.ElementAt(i).ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expr, i);
                    return;
                }
            }
        }

        private void ResolveFunction(Expr.Function function, FunctionType type)
        {
            var enclosingFunction = _currentFunction;
            _currentFunction = type;

            BeginScope();
            foreach (var param in function.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);
            EndScope();

            _currentFunction = enclosingFunction;
        }

        private void Declare(Token name)
        {
            if (_scopes.Count == 0) return;

            var scope = _scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
            {
                Lox.Error(name, $"The variable {name.Lexeme} is already declared in the current scope.");
            }
            scope[name.Lexeme] = false;
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0) return;
            _scopes.Peek()[name.Lexeme] = true;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }
            Define(stmt.Name);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.Expr);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);
            ResolveFunction(stmt.FunctionExpression, FunctionType.Function);
            return null;
        }

        public object VisitFunctionExpr(Expr.Function expr)
        {
            ResolveFunction(expr, FunctionType.Function);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Expr);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if (_currentFunction == FunctionType.None)
            {
                Lox.Error(stmt.Keyword, "Can't return from top-level code.");
            }

            if (stmt.Value != null)
            {
                if (_currentFunction == FunctionType.Initializer)
                {
                    Lox.Error(stmt.Keyword, "Can't return a value from an initializer.");
                }
                Resolve(stmt.Value);
            }
            return null;
        }


        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null)
            {
                Resolve(stmt.ElseBranch);
            }
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.Class;

            Declare(stmt.Name);
            Define(stmt.Name);

            if (stmt.Superclass != null)
            {
                if (stmt.Name.Lexeme.Equals(stmt.Superclass.Name.Lexeme, StringComparison.Ordinal))
                {
                    Lox.Error(stmt.Superclass.Name, "A class can't inherit from itself.");
                }

                _currentClass = ClassType.Subclass;
                Resolve(stmt.Superclass);

                BeginScope();
                _scopes.Peek().Add("super", true);
            }

            BeginScope();
            _scopes.Peek()["this"] = true;

            foreach (var method in stmt.Methods)
            {
                var declaration = FunctionType.Method;
                if (method.Name.Lexeme == "init")
                {
                    declaration = FunctionType.Initializer;
                }
                ResolveFunction(method.FunctionExpression, declaration);
            }

            EndScope();

            if (stmt.Superclass != null)
            {
                EndScope();
            }

            _currentClass = enclosingClass;

            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Obj);
            return null;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            if (_currentClass == ClassType.None)
            {
                Lox.Error(expr.Keyword, "Can't use 'super' outside of a class.");
            }
            else if (_currentClass != ClassType.Subclass)
            {
                Lox.Error(expr.Keyword, "Can't user 'super' in a class with no superclass.");
            }

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if (_currentClass == ClassType.None)
            {
                Lox.Error(expr.Keyword, "Can't use 'this' outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            if (_scopes.Count > 0)
            {
                var isDeclared = _scopes.Peek().TryGetValue(expr.Name.Lexeme, out var isDefined);
                if (isDeclared && !isDefined)
                {
                    Lox.Error(expr.Name, "Can't read local variable in its own initializer.");
                }
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Callee);
            foreach (var argument in expr.Arguments)
            {
                Resolve(argument);
            }
            return null;
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.Obj);
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expr);
            return null;
        }
    }
}
