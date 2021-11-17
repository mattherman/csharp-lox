using System.Collections.Generic;

namespace Lox
{
    public class LoxClass : LoxInstance, ILoxCallable
    {
        public string Name { get; }
        public LoxClass Superclass { get; }
        private readonly Dictionary<string, LoxFunction> _methods;

        public LoxClass(LoxClass metaclass, string name, LoxClass superclass, Dictionary<string, LoxFunction> methods) : base(metaclass)
        {
            Name = name;
            Superclass = superclass;
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
            return found ? method : Superclass?.FindMethod(name);
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