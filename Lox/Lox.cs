using System;
using System.IO;

namespace Lox
{
    public class Lox
    {
        private static bool _hadError = false;

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: Lox [script]");
                Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }

            Environment.Exit(0);
        }

        private static void RunFile(string filename)
        {
            var source = File.ReadAllText(filename);
            Run(source);
            if (_hadError)
                Environment.Exit(65);
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (input == null) break;
                Run(input);
                _hadError = false;
            }
        }

        private static void Run(string source)
        {
            var scanner = new Scanner(source);
            var tokens = scanner.ScanTokens();

            var parser = new Parser(tokens);
            var expression = parser.Parse();

            if (_hadError) return;

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        internal static void Error(int line, string message)
        {
            Report(line, string.Empty, message);
        }

        internal static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error{where}: {message}");
            _hadError = true;
        }
    }
}
