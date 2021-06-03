using System;
using System.Collections.Generic;

namespace Lox
{
    public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();

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

        private void Resolve(IList<Stmt> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            throw new System.NotImplementedException();
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            throw new System.NotImplementedException();
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            throw new System.NotImplementedException();
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            throw new System.NotImplementedException();
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            throw new System.NotImplementedException();
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            throw new System.NotImplementedException();
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            throw new System.NotImplementedException();
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            throw new System.NotImplementedException();
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            throw new System.NotImplementedException();
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            throw new System.NotImplementedException();
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            throw new System.NotImplementedException();
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            throw new System.NotImplementedException();
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            throw new System.NotImplementedException();
        }

        private void Declare(Token name)
        {
            if (_scopes.Count == 0) return;

            var scope = _scopes.Peek();
            scope.Add(name.Lexeme, false);
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0) return;
            _scopes.Peek()[name.Lexeme] = true;
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
            throw new System.NotImplementedException();
        }
    }
}
