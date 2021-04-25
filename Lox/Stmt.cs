using System.Collections.Generic;

namespace Lox
{
    public abstract class Stmt
    {
        public abstract T Accept<T>(IVisitor<T> visitor);

        public interface IVisitor<T>
        {
            T VisitIfStmt(If stmt);
            T VisitBlockStmt(Block stmt);
            T VisitExpressionStmt(Expression stmt);
            T VisitPrintStmt(Print stmt);
            T VisitVarStmt(Var stmt);
        }

        public class If : Stmt
        {
            public Expr Condition { get; }
            public Stmt ThenBranch { get; }
            public Stmt ElseBranch { get; }

            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitIfStmt(this);
            }
        }

        public class Block : Stmt
        {
            public List<Stmt> Statements { get; }

            public Block(List<Stmt> statements)
            {
                Statements = statements;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }
        }

        public class Expression : Stmt
        {
            public Expr Expr { get; }

            public Expression(Expr expr)
            {
                Expr = expr;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class Print : Stmt
        {
            public Expr Expr { get; }

            public Print(Expr expr)
            {
                Expr = expr;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

        public class Var : Stmt
        {
            public Token Name { get; }
            public Expr Initializer { get; }

            public Var(Token name, Expr initializer)
            {
                Name = name;
                Initializer = initializer;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }
    }
}
