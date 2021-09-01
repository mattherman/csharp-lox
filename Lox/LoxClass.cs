using System.Collections.Generic;

namespace Lox
{
    public class LoxClass : ILoxCallable
    {
        public string Name { get; }
        private readonly Dictionary<string, LoxFunction> _methods;

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            _methods = methods;
        }

        public int Arity => 0;

        public LoxFunction FindMethod(string name)
        {
            var found = _methods.TryGetValue(name, out var method);
            return found ? method : null;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);
            return instance;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}