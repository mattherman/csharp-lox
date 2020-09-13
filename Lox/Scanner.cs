namespace Lox
{
    public class Scanner
    {
        private readonly string _source;

        public Scanner(string source)
        {
            _source = source;
        }

        public string[] ScanTokens()
        {
            return _source.Split(' ');
        }
    }
}
