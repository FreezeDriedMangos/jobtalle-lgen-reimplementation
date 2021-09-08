

using LGen.LParse;
using System;
using System.Collections.Generic;

namespace LGen.LEvolve
{
    public class Mutator
    {
        public LSystem Mutate(LSystem system, MutationProfile profile, Randomizer randomizer)
        {
            system = new LSystem(system);
            
            GeneratedSymbols E = new GeneratedSymbols(system);
            Sentence axiom = Mutate(system.Axiom, profile, randomizer, E);
            
            for(int i = 0; i < system.Rules.Count; i++)
            {
                system.Rules[i] = Mutate(system.Rules[i], profile, randomizer, E);

                if (randomizer.MakeFloat(0,1) < profile.pRuleDuplicate) system.Rules.Add(new Rule(system.Rules[i]));
                if (randomizer.MakeFloat(0,1) < profile.pRuleRemove)    system.Rules.RemoveAt(i--); // the -- is here because otherwise we'd skip over the next rule
                if (randomizer.MakeFloat(0,1) < profile.pRuleAdd)       system.Rules.Add(new Rule("", ""));
            }

            return new LSystem(axiom, system.Rules);
        }

        private Rule Mutate(Rule rule, MutationProfile profile, Randomizer randomizer, GeneratedSymbols E)
        {
            Sentence lhs = Mutate(rule.LHS, profile, randomizer, E, true);
            Sentence rhs = Mutate(rule.RHS, profile, randomizer, E, false);
            return new Rule(lhs, rhs);
        }

        private Sentence Mutate(Sentence sentence, MutationProfile profile, Randomizer randomizer, GeneratedSymbols E, bool isRuleLHS = false)
        {
            Sentence oldSentence = sentence;
            sentence = new Sentence(sentence);    
            
            if (sentence.Tokens.Count == 0)
            {
                if (randomizer.MakeFloat(0,1) < profile.pSymbolAdd)    AddSymbolAt(sentence, 0, profile, randomizer, E, isRuleLHS, true);
                if (randomizer.MakeFloat(0,1) < profile.pBranchAdd)    sentence.Tokens.AddRange(new Token[]{ Legend.BRANCH_OPEN, Legend.BRANCH_CLOSE });
                if (randomizer.MakeFloat(0,1) < profile.pLeafAdd)      sentence.Tokens.AddRange(new Token[]{ Legend.LEAF_OPEN, Legend.BRANCH_CLOSE });
            }

            for(int i = 0; i < sentence.Tokens.Count; i++)
            {
                if (randomizer.MakeFloat(0,1) < profile.pSymbolAdd)    AddSymbolAt(sentence, i, profile, randomizer, E, isRuleLHS);
                if (randomizer.MakeFloat(0,1) < profile.pSymbolRemove) RemoveSymbolAt(sentence, i);
                if (randomizer.MakeFloat(0,1) < profile.pBranchAdd)    AddBranchAt(sentence, i);
                if (randomizer.MakeFloat(0,1) < profile.pBranchRemove) RemoveBranchAt(sentence, i);
                if (randomizer.MakeFloat(0,1) < profile.pLeafAdd)      AddLeafAt(sentence, i);
                if (randomizer.MakeFloat(0,1) < profile.pLeafRemove)   RemoveLeafAt(sentence, i);
            }

            return sentence.Tokens.Count > profile.sentenceSizeLimit ? oldSentence : sentence;
        }

        //
        // Utils
        //

        private Token MakeToken(Randomizer randomizer, MutationProfile profile, GeneratedSymbols E = null)
        {
            float rand = randomizer.MakeFloat(0, 1);

            if (rand < profile.pSymbolChanceRotation)
                return E == null
                    ? new Token[] { 
                        Legend.PITCH_INCREMENT, 
                        Legend.PITCH_DECREMENT, 
                        Legend.ROLL_INCREMENT, 
                        Legend.ROLL_DECREMENT, 
                        Legend.YAW_INCREMENT, 
                        Legend.YAW_DECREMENT, 
                        }[randomizer.MakeInt_Exclusive(0, 6)]
                    : E.rotations.Count == 0 ? null : E.rotations[randomizer.MakeInt_Exclusive(0, E.rotations.Count)]
                ;
            rand -= profile.pSymbolChanceRotation;

            if (rand < profile.pSymbolChanceSeed)
                return E == null
                    ? Legend.SEED
                    : E.seeds.Count == 0 ? null : E.seeds[randomizer.MakeInt_Exclusive(0, E.seeds.Count)]
                ;
            rand -= profile.pSymbolChanceSeed;
            
            if (rand < profile.pSymbolChanceStep)
                return E == null
                    ? (char)randomizer.MakeInt_Inclusive(Legend.STEP_MIN, Legend.STEP_MAX)
                    : E.steps.Count == 0 ? null : E.steps[randomizer.MakeInt_Exclusive(0, E.steps.Count)]
                ;
            rand -= profile.pSymbolChanceStep;

            if (true)
                return E == null
                    ? (char)randomizer.MakeInt_Inclusive(Legend.CONST_MIN, Legend.CONST_MAX)
                    : E.constants.Count == 0 ? null : E.constants[randomizer.MakeInt_Exclusive(0, E.constants.Count)]
                ;

            throw new Exception("Token failed to generate!");
        }

        private bool InsideLeaf(Sentence sentence, int i)
        {
            if (sentence.Tokens[i] == Legend.LEAF_OPEN) return true;
            if (sentence.Tokens[i] == Legend.BRANCH_CLOSE) return sentence.Tokens[FindMatchingOpenBracket(sentence, i)] == Legend.LEAF_OPEN;

            // go backwards and if you find an unmatched <, return true

            // <A[B|]]
            // <A][B|]
            // <A][[<C]B|]]

            int depth = 0;
            int minDepth = 0;
            for(int j = i; j >= 0; j--)
            {
                if (sentence.Tokens[j] == Legend.LEAF_OPEN && depth <= minDepth) return true;

                if (sentence.Tokens[j] == Legend.BRANCH_OPEN || sentence.Tokens[j] == Legend.LEAF_OPEN) depth--;
                if (sentence.Tokens[j] == Legend.BRANCH_CLOSE) depth++;

                minDepth = Math.Min(depth, minDepth);
            }

            return false;
        }

        private int FindMatchingCloseBracket(Sentence sentence, int i)
        {
            int depth = 0;
            for(int j = i; j < sentence.Tokens.Count; j++)
            {
                if (sentence.Tokens[j] == Legend.BRANCH_OPEN || sentence.Tokens[j] == Legend.LEAF_OPEN) depth++;
                if (sentence.Tokens[j] == Legend.BRANCH_CLOSE) depth--;

                if (depth == 0) return j;
            }

            throw new Exception("Invalid sentence with unmatched bracket at " + i + ": " + sentence);
        }

        private int FindMatchingOpenBracket(Sentence sentence, int i)
        {
            int depth = 0;
            for(int j = i; j >= 0; j--)
            {
                if (sentence.Tokens[j] == Legend.BRANCH_OPEN || sentence.Tokens[j] == Legend.LEAF_OPEN) depth--;
                if (sentence.Tokens[j] == Legend.BRANCH_CLOSE) depth++;

                if (depth == 0) return j;
            }

            throw new Exception("Invalid sentence with unmatched bracket at " + i + ": " + sentence);
        }

        //
        // Sentence mutations
        //

        private void AddSymbolAt(Sentence sentence, int i, MutationProfile profile, Randomizer randomizer, GeneratedSymbols E, bool forceE = false, bool forceBehind = false)
        {
            int insertAt = (forceBehind || randomizer.MakeFloat(0, 1) < profile.pAddSymbolBehind) ? i : i+1;
            GeneratedSymbols useE = (forceE || randomizer.MakeFloat(0, 1) < 1-profile.pSymbolChanceNew) ? E : null;
            Token token = MakeToken(randomizer, profile, useE);
            if (token == null) return;
            sentence.Tokens.Insert(insertAt, token);
        }

        private void RemoveSymbolAt(Sentence sentence, int i)
        {
            Token t = sentence.Tokens[i];
            if(t == Legend.BRANCH_OPEN) return;
            if(t == Legend.BRANCH_CLOSE) return;
            if(t == Legend.LEAF_OPEN) return;

            sentence.Tokens.RemoveAt(i);
        }
        
        private void AddBranchAt(Sentence sentence, int i) { AddBranchAt(sentence, i, Legend.BRANCH_OPEN); }
        private void AddBranchAt(Sentence sentence, int i, Token openSymbol)
        { 
            // if i is a branch open, surround whole branch
            Token t = sentence.Tokens[i];
            if(t == Legend.BRANCH_OPEN || t == Legend.LEAF_OPEN)
            {
                int close = FindMatchingCloseBracket(sentence, i);
                int open = i;
                
                sentence.Tokens.Insert(close+1, Legend.BRANCH_CLOSE);
                sentence.Tokens.Insert(open, openSymbol);
            }
            else if(t == Legend.BRANCH_CLOSE)
            {
                int open = FindMatchingOpenBracket(sentence, i);
                int close = i;
        
                sentence.Tokens.Insert(close+1, Legend.BRANCH_CLOSE);
                sentence.Tokens.Insert(open, openSymbol);
            }
            else
            {
                // insert ] at i+1
                // then insert [ at i
                sentence.Tokens.Insert(i+1, Legend.BRANCH_CLOSE);
                sentence.Tokens.Insert(i, openSymbol);
            }
        }

        private void AddLeafAt(Sentence sentence, int i)
        { 
            // if inside leaf, do not add a sub leaf, that's not allowed
            if (InsideLeaf(sentence, i)) return;

            AddBranchAt(sentence, i, Legend.LEAF_OPEN);
        }

        private void RemoveBranchOrLeafAt(Sentence sentence, int i)
        {
            // if not branch open, fail
            if (sentence.Tokens[i] != Legend.BRANCH_OPEN && sentence.Tokens[i] != Legend.LEAF_OPEN) return;

            int open = i;
            int close = FindMatchingCloseBracket(sentence, i);
            sentence.Tokens.RemoveAt(close);
            sentence.Tokens.RemoveAt(open);
        }

        private void RemoveBranchAt(Sentence sentence, int i)
        {
            if (sentence.Tokens[i] != Legend.BRANCH_OPEN) return;
            RemoveBranchOrLeafAt(sentence, i);
        }

        private void RemoveLeafAt(Sentence sentence, int i)
        {
            if (sentence.Tokens[i] != Legend.LEAF_OPEN) return;
            RemoveBranchOrLeafAt(sentence, i);
        }
        
    }

    
    class GeneratedSymbols
    {
        public List<Token> rotations = new List<Token>();
        public List<Token> seeds = new List<Token>();
	    public List<Token> steps = new List<Token>();
        public List<Token> constants = new List<Token>();

        public GeneratedSymbols(LSystem system)
        {
            List<Token> allTokens = new List<Token>();
            allTokens.AddRange(system.Axiom.Tokens);
            foreach (Rule r in system.Rules) allTokens.AddRange(r.RHS.Tokens);

            foreach (Token t in allTokens)
            {
                if (t == Legend.BRANCH_OPEN) continue;
                if (t != Legend.BRANCH_CLOSE) continue;
                if (t != Legend.LEAF_OPEN) continue;

                if (t.OnRangeInclusive(Legend.CONST_MIN, Legend.CONST_MAX)) this.constants.Add(t);
                if (t.OnRangeInclusive(Legend.STEP_MIN, Legend.STEP_MAX)) this.steps.Add(t);
                if (t == Legend.SEED) this.seeds.Add(t);
                if 
                (
                    t == Legend.PITCH_INCREMENT ||
                    t == Legend.PITCH_DECREMENT ||
                    t == Legend.ROLL_INCREMENT ||
                    t == Legend.ROLL_DECREMENT ||
                    t == Legend.YAW_INCREMENT ||
                    t == Legend.YAW_DECREMENT
                ) this.rotations.Add(t);
            }
        }
    }
}


