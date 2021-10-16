using LGen.LParse;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// note: this file is only used for display and fun stuff. not used for any calculations
namespace LGen.LSimulate { 
    public class Speciation
    {
		public static float Delta(Agent a, Agent b, float c1, float c2, float c3)
        {
			// NEAT style speciation
			// line up rules by id (innovation id)
			// compare rules with same id using LCS
			// finally, compare axioms with LCS

			// formula:
			// d = c1*E/N + c2*D/N + c3W
			// where E is number of Excess genes
			// D is number of disjoint genes
			// W is the difference between matching genes
			// N is the number of genes in the larger genome
			//
			// for this purpose, read "gene" as "rule"

			float numExcess = 0;
			float numDisjoint = 0;
			float numCommon = 1; // 1 for the axiom, it's always in common between two systems
			float totalSentenceDifference = Difference(a.system.Axiom, b.system.Axiom);
		
			int aSize = a.system.Rules.Count;
			int bSize = b.system.Rules.Count;
			int i;
			int j;
			for(i = j = 0; i < aSize || j < bSize; ) {
				Rule ra = a.system.Rules[Mathf.Min(i, aSize-1)]; 
				Rule rb = b.system.Rules[Mathf.Min(j, bSize-1)]; 
			
				if(ra.id == rb.id) {
					numCommon++;
					totalSentenceDifference += Difference(ra, rb);
				
					i++;
					j++;	
				} else if (j >= bSize) { // case 1a
					numExcess++;
					i++;
				} else if (i >= aSize) { // case 2a
					numExcess++;
					j++;
				} else if (ra.id > rb.id) { // case 1b
					// This is the case where g1 is disjoint/excess
			
					if(j < bSize) {
						// g1 is disjoint
						numDisjoint++;				
					} else {
						// g1 is excess
						numExcess++;
					}
				
					i++;
				} else if (ra.id < rb.id) { // case 2b
					// This is the case where g2 is disjoint/excess
			
					if(i < aSize) {
						// g2 is disjoint
						numDisjoint++;				
					} else {
						// g2 is excess
						numExcess++;
					}
				
					j++;
				} else {} // Not possible
			}
		
			//System.out.printf("numDisjoint: %d, numExcess: %d\n", numDisjoint, numExcess);
			//System.out.printf("g1Size: %d, g2Size: %d\n", g1Size, g2Size);
		
			float n = Mathf.Max(aSize, bSize) + 1; // +1 for axiom
			
			return c1*numExcess/n + c2*numDisjoint/n + c3*totalSentenceDifference;
        }

		public static int Difference(Rule ra, Rule rb)
        {
			string ras = ra.ToString();
			string rbs = rb.ToString();
			return Mathf.Max(ras.Length, rbs.Length) - LongestCommonSubsequence(ras, rbs);
        }
		public static int Difference(Sentence ra, Sentence rb)
        {
			string ras = ra.ToString();
			string rbs = rb.ToString();
			return Mathf.Max(ras.Length, rbs.Length) - LongestCommonSubsequence(ras, rbs);
        }

		// function from https://www.programmingalgorithms.com/algorithm/longest-common-subsequence/
        public static int LongestCommonSubsequence(string s1, string s2/*, out string output*/)
		{
			int i, j, k, t;
			int s1Len = s1.Length;
			int s2Len = s2.Length;
			int[] z = new int[(s1Len + 1) * (s2Len + 1)];
			int[,] c = new int[(s1Len + 1), (s2Len + 1)];

			for (i = 0; i <= s1Len; ++i)
				c[i, 0] = z[i * (s2Len + 1)];

			for (i = 1; i <= s1Len; ++i)
			{
				for (j = 1; j <= s2Len; ++j)
				{
					if (s1[i - 1] == s2[j - 1])
						c[i, j] = c[i - 1, j - 1] + 1;
					else
						c[i, j] = Mathf.Max(c[i - 1, j], c[i, j - 1]);
				}
			}

			t = c[s1Len, s2Len];
			char[] outputSB = new char[t];

			for (i = s1Len, j = s2Len, k = t - 1; k >= 0;)
			{
				if (s1[i - 1] == s2[j - 1])
				{
					outputSB[k] = s1[i - 1];
					--i;
					--j;
					--k;
				}
				else if (c[i, j - 1] > c[i - 1, j])
					--j;
				else
					--i;
			}

			//output = new string(outputSB);

			return t;
		}
    }
}