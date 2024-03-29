﻿
using System.Collections.Generic;

namespace LGen.LParse 
{ 
    public class LSystem
    {
        private static char[] BRANCH_TOKENS = new char[]{
	        Legend.LEAF_OPEN,
	        Legend.BRANCH_OPEN,
	        Legend.BRANCH_CLOSE
        };

        public Sentence Axiom { get; set; }
        public List<Rule> Rules { get; set; }

        public LSystem(Sentence Axiom, List<Rule> Rules) { this.Axiom = Axiom; this.Rules = Rules; }
		public LSystem(LSystem system)
        {
			this.Axiom = new Sentence(system.Axiom);
			this.Rules = new List<Rule>();
			foreach(Rule r in system.Rules) this.Rules.Add(new Rule(r));
        }
		public LSystem(string axiom, string[] rules)
        {
			this.Axiom = new Sentence(axiom);
			this.Rules = new List<Rule>();
			foreach(string r in rules) this.Rules.Add(new Rule(r));
        }
		public LSystem(string s)
        {
			string[] strings = s.Split('\n');
			this.Axiom = strings[0];

			this.Rules = new List<Rule>();
			for(int i = 1; i < strings.Length; i++) this.Rules.Add(strings[i]);
        }

		public Sentence Generate(GrowthProfile growthProfile, Randomizer randomizer)
		{ 
			int iterations = randomizer.MakeInt_Inclusive(1, growthProfile.Iterations);
			Sentence sentence = new Sentence(this.Axiom);

			for(int application = 0; application < iterations; application++)
				if(!sentence.Apply(this.Rules, randomizer, growthProfile.GetGrowth(application), growthProfile.GetMaxNumForwardSymbols(application)))
					break;
				

			return sentence;
		}

		public override string ToString()
        {
			return this.Axiom + "\n" + string.Join("\n", this.Rules);
        }

		//std::vector<Token> System::getGeneratedTokens() const {
		//	std::vector<Token> tokens;

		//	getGeneratedTokens(tokens, axiom);

		//	for(const auto &rule : rules) {
		//		getGeneratedTokens(tokens, rule.getLhs());
		//		getGeneratedTokens(tokens, rule.getRhs());
		//	}

		//	return tokens;
		//}

		//void System::getGeneratedTokens(std::vector<Token>& tokens, const Sentence& sentence) const {
		//	for(const auto &token : sentence.getTokens())
		//		if(std::find(
		//			std::begin(branchTokens),
		//			std::end(branchTokens),
		//			token.getSymbol()) == std::end(branchTokens))
		//			tokens.emplace_back(token.getSymbol());
		//}


    }
}