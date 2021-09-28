
using System;

namespace LGen.LParse 
{ 
    public class Token
    {
        public char Symbol { get; private set; }

        public Token(char s) { this.Symbol = s; }
        
        public static bool operator ==(Token t, Token o) => (t is null && o is null) || (!(t is null) && !(o is null) && t.Symbol == o.Symbol);
        public static bool operator !=(Token t, Token o) => ((t is null || o is null) && !object.ReferenceEquals(t, o)) || t.Symbol != o.Symbol;
        public static implicit operator Token(char c) => new Token(c);

        public static Token Clone(Token t) { return new Token(t.Symbol); }

        public override string ToString() { return this.Symbol+""; }

        public bool OnRangeInclusive(char min, char max)
        {
            return min <= this.Symbol && this.Symbol <= max;
        }
    }

}
