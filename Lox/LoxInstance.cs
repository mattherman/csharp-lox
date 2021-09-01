using System.Collections.Generic;

namespace Lox
{
    public class LoxInstance
    {
        private LoxClass _klass;
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            _klass = klass;
        }

        public object Get(Token name)
        {
            if (_fields.ContainsKey(name.Lexeme))
            {
                return _fields[name.Lexeme];
            }

            var method = _klass.FindMethod(name.Lexeme);
            if (method != null)
            {
                return method;
            }
            
            throw new RuntimeException(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object val)
        {
            _fields[name.Lexeme] = val;
        }

        public override string ToString()
        {
            return $"{_klass.Name} instance";
        }
    }
}