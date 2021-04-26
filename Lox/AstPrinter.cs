using System.Linq;
using System.Text;

namespace Lox
{
    public class AstPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        private string Parenthesize(string name, params Expr[] exprs)
        {
            var builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (var expr in exprs)
            {
                builder.Append($" {expr.Accept(this)}");
            }
            builder.Append(")");

            return builder.ToString();
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return Parenthesize(expr.Name.Lexeme, expr.Value);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.Expr);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value?.ToString() ?? "nil";
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Right);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return expr.Name.Lexeme;
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
        }

        public string VisitCallExpr(Expr.Call expr)
        {
            var expressions = new []{ expr.Callee }.Concat(expr.Arguments);
            return Parenthesize("call", expressions.ToArray());
        }
    }
}
