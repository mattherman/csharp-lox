using System.Collections.Generic;

namespace Lox
{
    public class Environment
    {
        private readonly IDictionary<string, object> values = new Dictionary<string, object>();

        public void Define(string name, object val)
        {
            values[name] = val;
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
