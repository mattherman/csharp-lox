using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class LoxFunction : ILoxCallable
    {
        private readonly Stmt.Function _declaration;

        public int Arity { get { return _declaration.Parameters.Count; } }

        public LoxFunction(Stmt.Function declaration)
        {
            _declaration = declaration;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(interpreter.Globals);
            foreach (var (parameter, argument) in _declaration.Parameters.Zip(arguments))
            {
                environment.Define(parameter.Lexeme, argument);
            } 

            interpreter.ExecuteBlock(_declaration.Body, environment);
            return null;
        }

        public override string ToString() => $"<fn {_declaration.Name.Lexeme}>";
    }
}
