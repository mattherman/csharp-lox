using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GenerateAst
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: GenerateAst <output_dir>");
                Environment.Exit(64);
            }
            var outputDir = args[0];
            DefineAst(outputDir, "Expr", new []{
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token op, Expr right",
                "Grouping : Expr expr",
                "Literal  : object value",
                "Unary    : Token op, Expr right",
                "Variable : Token name"
            });
            DefineAst(outputDir, "Stmt", new []{
                "Expression : Expr expr",
                "Print      : Expr expr",
                "Var        : Token name, Expr initializer"
            });
        }

        private static void DefineAst(string outputDir, string baseName, IEnumerable<string> types)
        {
            var lines = new List<string>();

            lines.Add("namespace Lox");
            lines.Add("{");
            
            lines.Add($"\tpublic abstract class {baseName}");
            lines.Add("\t{");
            lines.Add("\t\tpublic abstract T Accept<T>(IVisitor<T> visitor);");
            
            DefineVisitor(lines, baseName, types);

            foreach (var type in types)
            {
                var typeParts = type.Split(":");
                var className = typeParts[0].Trim();
                var fields = typeParts[1].Trim();

                lines.Add(string.Empty);
                DefineType(lines, baseName, className, fields);
            }
            
            lines.Add("\t}");

            lines.Add("}");

            var path = $"{outputDir}/{baseName}.cs";
            File.WriteAllLines(path, lines);
        }

        private static void DefineVisitor(IList<string> lines, string baseName, IEnumerable<string> types)
        {
            lines.Add(string.Empty);
            lines.Add("\t\tpublic interface IVisitor<T>");
            lines.Add("\t\t{");

            foreach (var type in types)
            {
                var typeParts = type.Split(":");
                var typeName = typeParts[0].Trim();
                lines.Add($"\t\t\tT Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            lines.Add("\t\t}");
        }

        private static void DefineType(IList<string> lines, string baseName, string className, string fieldList)
        {
            lines.Add($"\t\tpublic class {className} : {baseName}");
            lines.Add("\t\t{");
            
            var fields = fieldList
                .Split(",")
                .Select(f => f.Trim())
                .Select(f => new Field(f));

            foreach (var field in fields)
            {
                lines.Add($"\t\t\tpublic {field.Type} {field.PascalCaseIdentifier} {{ get; }}");
            }

            var parameterList = String.Join(", ", fields.Select(f => f.ToParam()));
            lines.Add(string.Empty);
            lines.Add($"\t\t\tpublic {className}({parameterList})");
            lines.Add("\t\t\t{");
            
            foreach (var field in fields)
            {
                lines.Add($"\t\t\t\t{field.PascalCaseIdentifier} = {field.CamelCaseIdentifier};");
            }

            lines.Add("\t\t\t}");

            lines.Add(string.Empty);
            lines.Add("\t\t\tpublic override T Accept<T>(IVisitor<T> visitor)");
            lines.Add("\t\t\t{");
            lines.Add($"\t\t\t\treturn visitor.Visit{className}{baseName}(this);");
            lines.Add("\t\t\t}");
            lines.Add("\t\t}");
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
