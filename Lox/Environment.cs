using System.Collections.Generic;

namespace Lox
{
    public class Environment
    {
        public Environment Enclosing { get; private set; }
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            Enclosing = enclosing;
        }

        public void Define(Token name, object val)
        {
            Define(name.Lexeme, val);
        }

        public void Define(string name, object val)
        {
            _values[name] = val;
        }

        public void Assign(Token name, object val)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = val;
                return;
            }

            throw new RuntimeException(name, $"Undefined variable {name.Lexeme}.");
        }

        public void AssignAt(int depth, string name, object val)
        {
            var ancestorEnvironment = Ancestor(depth);
            ancestorEnvironment._values[name] = val;
        }

        public object Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out var val))
                return val;

            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public object GetAt(int depth, string name)
        {
            var ancestorEnvironment = Ancestor(depth);
            return ancestorEnvironment._values[name];
        }

        private Environment Ancestor(int depth)
        {
            var environment = this;
            for (var i = 0; i < depth; i++)
            {
                environment = environment.Enclosing;
            }
            return environment;
        }
    }
}
