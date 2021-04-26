using System.Collections.Generic;

namespace Lox
{
    public interface ILoxCallable
    {
        int Arity { get; }
        object Call(Interpreter interpreter, List<object> arguments);
    }
}
