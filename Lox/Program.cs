using System;
using System.IO;

namespace Lox
{
    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: Lox [script]");
                return 64;
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }

            return 0;
        }

        private static void RunFile(string filename)
        {
            var source = File.ReadAllText(filename);
            Run(source);
        }

        private static void RunPrompt()
        {
            while(true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (input == null) break;
                Run(input);
            }
        }

        private static void Run(string source)
        {
            var scanner = new Scanner(source);
            foreach (var token in scanner.ScanTokens())
            {
                Console.WriteLine(token);
            }
        }
    }
}
