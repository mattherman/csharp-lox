using System.Collections.Generic;

namespace Lox
{
    public class Environment
    {
        private readonly Environment _enclosing;
        private readonly IDictionary<string, object> values = new Dictionary<string, object>();

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
            values[name] = val;
        }

        public void Assign(Token name, object val)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = val;
                return;
            }

            if (_enclosing != null)
            {
                _enclosing.Assign(name, val);
                return;
            }

            throw new RuntimeException(name, $"Undefined variable {name.Lexeme}.");
        }

        public object Get(Token name)
        {
            var found = values.TryGetValue(name.Lexeme, out var val);

            if (found)
            {
                if (val == null)
                {
                    throw new RuntimeException(name, $"Attempted to access unassigned variable '{name.Lexeme}'.");
                }
                return val;
            }
            else if (_enclosing != null)
            {
                return _enclosing.Get(name);
            }

            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
