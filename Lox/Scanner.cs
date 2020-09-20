using System;
using System.Collections.Generic;

namespace Lox
{
    public class Scanner
    {
        private readonly string _source;
        private readonly IList<Token> _tokens = new List<Token>();

        private bool IsAtEnd() => _current >= _source.Length;

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        public Scanner(string source)
        {
            _source = source;
        }

        public IList<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
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
                case '/':
                    if (Match('/'))
                    {
                        // If it is a comment, we want to consume the entire line
                        while (Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    break;
                case '\n':
                    _line++;
                    break;
                case '"':
                    String();
                    break;
                default:
                    Lox.Error(_line, "Unexpected character.");
                    break;
            }
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Lox.Error(_line, "Unterminated string.");
                return;
            }

            Advance();

            // Length is difference between _current and _start minus the two quotes
            var length = (_current - _start) - 2;
            var value = _source.Substring(_start + 1, length);
            AddToken(TokenType.STRING, value);
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[_current];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            return true;
        }

        private char Advance()
        {
            _current++;
            return _source[_current - 1];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            var text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }
    }
}
