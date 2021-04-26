using System;
using System.Collections.Generic;

namespace Lox
{
    public class Clock : ILoxCallable
    {
        public int Arity { get { return 0; } }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}
