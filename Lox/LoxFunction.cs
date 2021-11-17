using System.Collections.Generic;
using System.Linq;

namespace Lox
{
    public class LoxFunction : ILoxCallable
    {
        private readonly string _name;
        private readonly Expr.Function _declaration;
        private readonly Environment _closure;
        private readonly bool _isInitializer;

        public int Arity { get { return _declaration.Parameters.Count; } }

        public LoxFunction(string name, Expr.Function declaration, Environment closure, bool isInitializer)
        {
            _name = name;
            _declaration = declaration;
            _closure = closure;
            _isInitializer = isInitializer;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            var environment = new Environment(_closure);
            environment.Define("this", instance);
            return new LoxFunction(_name, _declaration, environment, _isInitializer);
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(_closure);
            foreach (var (parameter, argument) in _declaration.Parameters.Zip(arguments))
            {
                environment.Define(parameter.Lexeme, argument);
            } 

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (Return returnValue)
            {
                if (returnValue.Value != null) return returnValue.Value;
            }

            if (_isInitializer)
                return _closure.GetAt(0, "this");

            return null;
        }

        public override string ToString() => 
            _name == null ? "<fn>" : $"<fn {_name}>";
    }
}
