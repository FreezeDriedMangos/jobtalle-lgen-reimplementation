

using System.Collections.Generic;

namespace LGen.LParse 
{ 
    public class Sentence 
    {
        public List<Token> Tokens { get; private set; }

        public Sentence(Token t) { Tokens = new List<Token>(); Tokens.Add(t); }
        public Sentence(List<Token> ts) { Tokens = new List<Token>(); Tokens.AddRange(ts); }
        public Sentence(string s) { Tokens = new List<Token>(); foreach (char c in s) Tokens.Add(c); }
		public Sentence(Sentence s) { Tokens = new List<Token>(); foreach (Token t in s.Tokens) Tokens.Add(Token.Clone(t)); }

        public static bool operator ==(Sentence s, Sentence n) => s.Equals(n);
        public static bool operator !=(Sentence s, Sentence n) => !s.Equals(n);
        public static implicit operator Sentence(string s) => new Sentence(s);

        public bool Equals(Sentence other)
        {
            if(this.Tokens.Count != other.Tokens.Count)
		        return false;

            for(int i = 0; i < this.Tokens.Count; i++)
		        if(this.Tokens[i] != other.Tokens[i])
			        return false;

	        return true;
        }

        public override string ToString() 
        {
            string s = "";
            foreach(Token t in Tokens) s += t;
            return s;
        }

		public bool Applicable(int at, Rule rule)
        {
			if (rule.LHS.Tokens.Count <= 0) return false; // can't apply a rule with an empty left hand side
			if (this.Tokens.Count - at < rule.LHS.Tokens.Count) return false;

			for (int i = at; i < this.Tokens.Count && i < rule.LHS.Tokens.Count + at; i++)
				if (this.Tokens[i] != rule.LHS.Tokens[i-at]) 
					return false;

			return true;
        }

		public bool Apply(List<Rule> rules, Randomizer randomizer, int limit, int limitForwardTokens=-1) 
		{
			List<Token> newTokens = new List<Token>();
			bool ruleWasApplied = false;

			for (int at = 0; at < this.Tokens.Count;)
			{
				List<Rule> possibleRules = new List<Rule>();
				
				foreach(Rule rule in rules)
					if(Applicable(at, rule))
						possibleRules.Add(rule);

				if(possibleRules.Count > 0)
				{
					Rule rule = possibleRules[randomizer.MakeInt_Exclusive(0, possibleRules.Count)];
					List<Token> result = rule.RHS.Tokens;
					
					foreach(Token t in result) newTokens.Add(Token.Clone(t));

					at += rule.LHS.Tokens.Count;
					ruleWasApplied = true;
				}
				else
				{
					newTokens.Add(Tokens[at]);
					at++;
				}

				if (newTokens.Count > limit) 
					return false;
			}

			// make sure there's not too many forward symbols
			if (limitForwardTokens > 0)
			{
				int numForward = 0;
				foreach(Token t in newTokens) if(t.OnRangeInclusive(Legend.STEP_MIN, Legend.STEP_MAX)) numForward++;

				if (numForward > limitForwardTokens) return false;
			}

			this.Tokens = newTokens;
			return ruleWasApplied;
		}

		public bool Validate()
        {
			int depth = 0;
			bool leafMode = false;
			foreach(Token t in Tokens)
			{
				if (t == Legend.BRANCH_OPEN || t == Legend.LEAF_OPEN) depth++;
				if (t == Legend.BRANCH_CLOSE) depth--;

				if (t == Legend.LEAF_OPEN)
					if (leafMode) return false;
					else leafMode = true;
			}

			return depth == 0;
        }
    }
}
