using System.Collections.Generic;

namespace Lox
{
    public class LoxInstance
    {
        private LoxClass _klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            _klass = klass;
        }

        public object Get(Token name)
        {
            var exists = fields.TryGetValue(name.Lexeme, out var field);

            if (!exists)
                throw new RuntimeException(name, $"Undefined property '{name.Lexeme}'.");

            return field;
        }

        public void Set(Token name, object val)
        {
            fields[name.Lexeme] = val;
        }

        public override string ToString()
        {
            return $"{_klass.Name} instance";
        }
    }
}