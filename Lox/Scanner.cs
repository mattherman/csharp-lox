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
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, line));
            return _tokens;
        }

        private void ScanToken()
        {
            switch (Advance())
            {
                case '(':
                    AddToken(TokenType.LEFT_PAREN);
                    break;
                case ')':
                    AddToken(TokenType.RIGHT_PAREN);
                    break;
                case '{':
                    AddToken(TokenType.LEFT_BRACE);
                    break;
                case '}':
                    AddToken(TokenType.RIGHT_BRACE);
                    break;
                case ',':
                    AddToken(TokenType.COMMA);
                    break;
                case '.':
                    AddToken(TokenType.DOT);
                    break;
                case '-':
                    AddToken(TokenType.MINUS);
                    break;
                case '+':
                    AddToken(TokenType.PLUS);
                    break;
                case ';':
                    AddToken(TokenType.SEMICOLON);
                    break;
                case '*':
                    AddToken(TokenType.STAR);
                    break;
                case '!':
                    AddToken(Match('=') ?
                        TokenType.BANG_EQUAL :
                        TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ?
                        TokenType.EQUAL_EQUAL :
                        TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ?
                        TokenType.LESS_EQUAL :
                        TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ?
                        TokenType.GREATER_EQUAL :
                        TokenType.GREATER);
                    break;
                default:
                    Lox.Error(line, "Unexpected character.");
                    break;
            }
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[current] != expected) return false;

            current++;
            return true;
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
            var text = _source.Substring(start, current - start);
            _tokens.Add(new Token(type, text, literal, line));
        }
    }
}
