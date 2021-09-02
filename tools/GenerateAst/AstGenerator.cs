using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenerateAst
{
    class AstGenerator
    {
        private const string Indent = "    ";
        private static readonly string DoubleIndent = Indent.Repeat(2);
        private static readonly string TripleIndent = Indent.Repeat(3);
        private static readonly string QuadIndent = Indent.Repeat(4);

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: GenerateAst <output_dir>");
                Environment.Exit(64);
            }
            var outputDir = args[0];
            DefineAst(
                outputDir, 
                "Expr",
                new [] {
                    "Assign   : Token name, Expr value",
                    "Binary   : Expr left, Token op, Expr right",
                    "Call     : Expr callee, Token paren, List<Expr> arguments",
                    "Get      : Expr obj, Token name",
                    "Grouping : Expr expr",
                    "Literal  : object value",
                    "Logical  : Expr left, Token op, Expr right",
                    "Set      : Expr obj, Token name, Expr value",
                    "This     : Token keyword",
                    "Unary    : Token op, Expr right",
                    "Variable : Token name"
                },
                new [] {
                    "System.Collections.Generic"
                }
            );
            DefineAst(
                outputDir,
                "Stmt",
                new [] {
                    "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                    "Block      : List<Stmt> statements",
                    "Class      : Token name, List<Stmt.Function> methods",
                    "Expression : Expr expr",
                    "Print      : Expr expr",
                    "Var        : Token name, Expr initializer",
                    "While      : Expr condition, Stmt body",
                    "Return     : Token keyword, Expr value",
                    "Function   : Token name, List<Token> parameters, List<Stmt> body"
                },
                new [] {
                    "System.Collections.Generic"
                }
            );
        }

        private static void DefineAst(string outputDir, string baseName, IEnumerable<string> types, IEnumerable<string> dependentNamespaces = null)
        {
            var lines = new List<string>();

            if (dependentNamespaces != null)
            {
                foreach (var dependentNamespace in dependentNamespaces)
                {
                    lines.Add($"using {dependentNamespace};");
                } 
                lines.Add(string.Empty);
            }

            lines.Add("namespace Lox");
            lines.Add("{");
            
            lines.Add($"{Indent}public abstract class {baseName}");
            lines.Add($"{Indent}{{");
            lines.Add($"{DoubleIndent}public abstract T Accept<T>(IVisitor<T> visitor);");
            
            DefineVisitor(lines, baseName, types);

            foreach (var type in types)
            {
                var typeParts = type.Split(":");
                var className = typeParts[0].Trim();
                var fields = typeParts[1].Trim();

                lines.Add(string.Empty);
                DefineType(lines, baseName, className, fields);
            }
            
            lines.Add($"{Indent}}}");

            lines.Add("}");

            var path = $"{outputDir}/{baseName}.cs";
            File.WriteAllLines(path, lines);
        }

        private static void DefineVisitor(IList<string> lines, string baseName, IEnumerable<string> types)
        {
            lines.Add(string.Empty);
            lines.Add($"{DoubleIndent}public interface IVisitor<T>");
            lines.Add($"{DoubleIndent}{{");

            foreach (var type in types)
            {
                var typeParts = type.Split(":");
                var typeName = typeParts[0].Trim();
                lines.Add($"{TripleIndent}T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            lines.Add($"{DoubleIndent}}}");
        }

        private static void DefineType(IList<string> lines, string baseName, string className, string fieldList)
        {
            lines.Add($"{DoubleIndent}public class {className} : {baseName}");
            lines.Add($"{DoubleIndent}{{");
            
            var fields = fieldList
                .Split(",")
                .Select(f => f.Trim())
                .Select(f => new Field(f));

            foreach (var field in fields)
            {
                lines.Add($"{TripleIndent}public {field.Type} {field.PascalCaseIdentifier} {{ get; }}");
            }

            var parameterList = String.Join(", ", fields.Select(f => f.ToParam()));
            lines.Add(string.Empty);
            lines.Add($"{TripleIndent}public {className}({parameterList})");
            lines.Add($"{TripleIndent}{{");
            
            foreach (var field in fields)
            {
                lines.Add($"{QuadIndent}{field.PascalCaseIdentifier} = {field.CamelCaseIdentifier};");
            }

            lines.Add($"{TripleIndent}}}");

            lines.Add(string.Empty);
            lines.Add($"{TripleIndent}public override T Accept<T>(IVisitor<T> visitor)");
            lines.Add($"{TripleIndent}{{");
            lines.Add($"{QuadIndent}return visitor.Visit{className}{baseName}(this);");
            lines.Add($"{TripleIndent}}}");
            lines.Add($"{DoubleIndent}}}");
        }
    }

    class Field
    {
        public string Type { get; }
        public string CamelCaseIdentifier { get; }
        public string PascalCaseIdentifier { get; }

        public string ToParam() => $"{Type} {CamelCaseIdentifier}";

        public Field(string field)
        {
            var fieldParts = field.Split(" ");
            Type = fieldParts[0];
            CamelCaseIdentifier = fieldParts[1];
            PascalCaseIdentifier = Char.ToUpper(CamelCaseIdentifier[0]) + CamelCaseIdentifier.Substring(1);
        }
    }
}
