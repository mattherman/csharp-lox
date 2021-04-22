using System.Collections.Generic;

namespace Lox
{
    public class Environment
    {
        private readonly IDictionary<string, object> values = new Dictionary<string, object>();

        public void Define(Token name, object val)
        {
            values[name.Lexeme] = val;
        }

        public void Assign(Token name, object val)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = val;
                return;
            }

            throw new RuntimeException(name, $"Undefined variable {name.Lexeme}.");
        }

        public object Get(Token name)
        {
            var found = values.TryGetValue(name.Lexeme, out var val);
            if (!found)
            {
                throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
            }
            return val;
        }
    }
}
