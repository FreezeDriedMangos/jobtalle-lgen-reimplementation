
using System;

namespace LGen.LParse 
{ 
    public class Rule
    {
        private static uint ID_COUNTER;
        public uint id = ID_COUNTER++;

        public const string CONNECTIVE = " => ";

        public Sentence LHS { get; private set; }
        public Sentence RHS { get; private set; }

        public Rule(Sentence LHS, Sentence RHS) { this.LHS = LHS; this.RHS = RHS; }
        public Rule(Rule r) { this.LHS = new Sentence(r.LHS); this.RHS = new Sentence(r.RHS); this.id = r.id; }

        public Rule(string s)
        {
            string[] strings = s.Split(new string[]{CONNECTIVE}, System.StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length != 2) throw new Exception("Cannot construct rule from invalid rule string.");

            this.LHS = strings[0];
            this.RHS = strings[1];
        }


        
        public static bool operator ==(Rule r, Rule u) => r.Equals(u);
        public static bool operator !=(Rule r, Rule u) => !r.Equals(u);
        public static implicit operator Rule(string s) => new Rule(s);
        public bool Equals(Rule other) { return this.LHS == other.LHS && this.RHS == other.RHS; }
        public override string ToString() { return this.LHS + CONNECTIVE + this.RHS; }
    }
}
