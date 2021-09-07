﻿
namespace LGen.LParse 
{ 
    public class Token
    {
        public char Symbol { get; private set; }

        public Token(char s) { this.Symbol = s; }
        
        public static bool operator ==(Token t, Token o) => t.Symbol == o.Symbol;
        public static bool operator !=(Token t, Token o) => t.Symbol != o.Symbol;
        public static implicit operator Token(char c) => new Token(c);

        public static Token Clone(Token t) { return new Token(t.Symbol); }

        public override string ToString() { return this.Symbol+""; }
    }

}
