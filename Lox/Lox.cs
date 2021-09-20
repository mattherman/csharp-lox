using System;
using System.IO;

namespace Lox
{
    public class Lox
    {
        private static Interpreter _interpreter = new Interpreter();
        private static string _filename;
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: Lox [script]");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }

            System.Environment.Exit(0);
        }

        private static void RunFile(string filename)
        {
            var source = File.ReadAllText(filename);
            _filename = filename;
            Run(source);
            if (_hadError)
                System.Environment.Exit(65);
            if (_hadRuntimeError)
                System.Environment.Exit(70);
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
            var statements = parser.Parse();

            if (_hadError) return;

            var resolver = new Resolver(_interpreter);
            resolver.Resolve(statements);

            if (_hadError) return;

            _interpreter.Interpret(statements);
        }

        internal static void RuntimeError(RuntimeException ex)
        {
            Error(ex.Token, ex.Message);
            _hadRuntimeError = true;
        }

        internal static void Error(int line, int column, string message)
        {
            Report(line, column, string.Empty, message);
        }

        internal static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, token.Column, " at end", message);
            }
            else
            {
                Report(token.Line, token.Column, $" at '{token.Lexeme}'", message);
            }
        }

        private static void Report(int line, int column, string where, string message)
        {
            var filename = _filename != null ? $"{_filename}, " : string.Empty;
            Console.WriteLine($"[{filename}line {line}, col {column}] Error{where}: {message}");
            _hadError = true;
        }
    }
}
