using System.Collections.Generic;

namespace Lox
{
    public class Environment
    {
        private readonly Environment _enclosing;
        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();

        public Environment()
        {
            _enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            _enclosing = enclosing;
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

        public void AssignAt(int depth, Token name, object val)
        {
            Ancestor(depth).Assign(name, val);
        }

        public object Get(Token name)
        {
            var found = _values.TryGetValue(name.Lexeme, out var val);

            if (found)
            {
                if (val == null)
                {
                    throw new RuntimeException(name, $"Attempted to access unassigned variable '{name.Lexeme}'.");
                }
                return val;
            }

            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public object GetAt(int depth, Token name)
        {
            return Ancestor(depth).Get(name);
        }

        private Environment Ancestor(int depth)
        {
            var environment = this;
            for (var i = 0; i < depth; i++)
            {
                environment = environment._enclosing;
            }
            return environment;
        }
    }
}
