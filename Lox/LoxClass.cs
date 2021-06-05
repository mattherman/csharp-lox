namespace Lox
{
    public class LoxClass
    {
        private readonly string _name;

        public LoxClass(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}