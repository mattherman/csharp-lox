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

        // This could probably be a prop initialized in the constructor
        // since it is not possible to change a class after definition.
        public int Arity
        {
            get
            {
                var initializer = FindMethod("init");
                if (initializer == null)
                    return 0;
                return initializer.Arity;
            }
        }

        public LoxFunction FindMethod(string name)
        {
            var found = _methods.TryGetValue(name, out var method);
            return found ? method : null;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LoxInstance(this);
            var initializer = FindMethod("init");
            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }
            return instance;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}