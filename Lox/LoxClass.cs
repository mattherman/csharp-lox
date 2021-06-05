using System.Collections.Generic;

namespace Lox
{
    public class LoxClass : ILoxCallable
    {
        public string Name { get; private set; }

        public LoxClass(string name)
        {
            Name = name;
        }

        public int Arity => 0;

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