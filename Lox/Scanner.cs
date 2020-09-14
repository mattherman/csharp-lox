using System.Collections.Generic;

namespace Lox
{
    public class Scanner
    {
        private readonly string _source;
        private readonly IList<Token> _tokens = new List<Token>();

        private bool IsAtEnd() => current >= _source.Length;

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            _source = source;
        }

        public IList<Token> ScanTokens()
        {
            while(!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, line));
            return _tokens;
        }

        private void ScanToken()
        {
            switch(Advance())
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                default:
                    Lox.Error(line, "Unexpected character.");
                    break;
            }
        }

        private char Advance()
        {
            current++;
            return _source[current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            var text = _source.Substring(start, current);
            _tokens.Add(new Token(type, text, literal, line));
        }
    }
}
