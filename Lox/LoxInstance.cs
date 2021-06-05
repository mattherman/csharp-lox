namespace Lox
{
    public class LoxInstance
    {
        private LoxClass _klass;

        public LoxInstance(LoxClass klass)
        {
            _klass = klass;
        }

        public override string ToString()
        {
            return $"{_klass.Name} instance";
        }
    }
}