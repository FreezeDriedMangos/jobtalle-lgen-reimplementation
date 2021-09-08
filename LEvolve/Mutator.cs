

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

            List<Token> E = new List<Token>();
            List<Token> N = new List<Token>();
            Sentence axiom = Mutate(system.Axiom, profile, randomizer, E, N);
            
            for(int i = 0; i < system.Rules.Count; i++)
            {
                if (randomizer.MakeFloat(0,1) < profile.duplicateRuleChance) system.Rules.Add(new Rule(system.Rules[i]));
                if (randomizer.MakeFloat(0,1) < profile.deleteRuleChance)    system.Rules.RemoveAt(i--); // the -- is here because otherwise we'd skip over the next rule
                if (randomizer.MakeFloat(0,1) < profile.createNewRuleChance) system.Rules.Add(new Rule("", ""));

                system.Rules[i] = Mutate(system.Rules[i], profile, randomizer, E, N);
            }

            return system;
        }

        public Rule Mutate(Rule rule, MutationProfile profile, Randomizer randomizer, List<Token> E, List<Token> N)
        {
            Sentence lhs = Mutate(rule.LHS, profile, randomizer, E, N, true);
            Sentence rhs = Mutate(rule.RHS, profile, randomizer, E, N, false);
            return new Rule(lhs, rhs);
        }

        public Sentence Mutate(Sentence sentence, MutationProfile profile, Randomizer randomizer, List<Token> E, List<Token> N, bool isRuleLHS = false)
        {
            Sentence oldSentence = sentence;
            sentence = new Sentence(sentence);    
            
            if (sentence.Tokens.Count == 0)
            {
                if (randomizer.MakeFloat(0,1) < profile.addSymbolChance)    AddSymbolAt(sentence, 0, profile, randomizer, E, N, isRuleLHS, true);
                if (randomizer.MakeFloat(0,1) < profile.addBranchChance)    sentence.Tokens.AddRange(new Token[]{ Legend.BRANCH_OPEN, Legend.BRANCH_CLOSE });
                if (randomizer.MakeFloat(0,1) < profile.addLeafChance)      sentence.Tokens.AddRange(new Token[]{ Legend.LEAF_OPEN, Legend.BRANCH_CLOSE });
            }

            for(int i = 0; i < sentence.Tokens.Count; i++)
            {
                if (randomizer.MakeFloat(0,1) < profile.addSymbolChance)    AddSymbolAt(sentence, i, profile, randomizer, E, N, isRuleLHS);
                if (randomizer.MakeFloat(0,1) < profile.removeSymbolChance) RemoveSymbolAt(sentence, i);
                if (randomizer.MakeFloat(0,1) < profile.addBranchChance)    AddBranchAt(sentence, i);
                if (randomizer.MakeFloat(0,1) < profile.removeBranchChance) RemoveBranchOrLeafAt(sentence, i);
                if (randomizer.MakeFloat(0,1) < profile.addLeafChance)      AddLeafAt(sentence, i);
                if (randomizer.MakeFloat(0,1) < profile.removeLeafChance)   RemoveBranchOrLeafAt(sentence, i);
            }

            return sentence.Tokens.Count > profile.sentenceSizeLimit ? oldSentence : sentence;
        }

        //
        // Utils
        //

        private List<Token> GetESet(LSystem system)
        {
            throw new NotImplementedException();
        }

        private List<Token> GetNSet()
        {
            throw new NotImplementedException();
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
            for(int j = i; j >= 0; j++)
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

        private void AddSymbolAt(Sentence sentence, int i, MutationProfile profile, Randomizer randomizer, List<Token> E, List<Token> N, bool forceE = false, bool forceBehind = false)
        {
            int insertAt = (forceBehind || randomizer.MakeFloat(0, 1) < profile.addSymbolBehindChance) ? i : i+1;
            List<Token> set = (forceE || randomizer.MakeFloat(0, 1) < profile.useEChance) ? E : N;
            sentence.Tokens.Insert(insertAt, set[randomizer.MakeInt_Exclusive(0, set.Count)]);
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

        private void RemoveBranchOrLeafAt(Sentence sentence, int i)
        {
            // if not branch open, fail
            if (sentence.Tokens[i] != Legend.BRANCH_OPEN && sentence.Tokens[i] != Legend.LEAF_OPEN) return;

            int open = i;
            int close = FindMatchingCloseBracket(sentence, i);
            sentence.Tokens.RemoveAt(close);
            sentence.Tokens.RemoveAt(open);
        }
        
        private void AddLeafAt(Sentence sentence, int i)
        { 
            // if inside leaf, do not add a sub leaf, that's not allowed
            if (InsideLeaf(sentence, i)) return;

            AddBranchAt(sentence, i, Legend.LEAF_OPEN);
        }
    }
}