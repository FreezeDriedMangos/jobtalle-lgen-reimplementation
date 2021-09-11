namespace LGen.LEvolve
{
    public class MutationProfile
    {
        public int sentenceSizeLimit = 32;

		public float pSymbolRemove = 0.005f;
		public float pSymbolAdd = 0.005f;
		public    float pSymbolChanceNew = 0.2f; // chance to use N instead of E
		public    float pAddSymbolBehind = 0.5f;
		public       float pSymbolChanceRotation = 0.35f; // These probabilities are
		public       float pSymbolChanceSeed = 0.05f;     // relative to each other,
		public       float pSymbolChanceStep = 0.4f;      // their total probability
		public       float pSymbolChanceConstant = 0.2f;  // is 100%.
		public float pBranchAdd = 0.002f;
		public float pBranchRemove = 0.002f * 2f;
		public float pLeafAdd = 0.002f;
		public float pLeafRemove = 0.002f * 2f;
		 
		public float pRuleDuplicate = 0.003f;
		public float pRuleAdd = 0.001f;
		public float pRuleRemove = 0.004f;

		public float pAxiomMutate = 1;//0.1f;


		public MutationProfile()
        {
			float sum = pSymbolChanceRotation + pSymbolChanceSeed + pSymbolChanceStep + pSymbolChanceConstant;
			pSymbolChanceRotation /= sum; 
			pSymbolChanceSeed 	  /= sum;
			pSymbolChanceStep 	  /= sum;
			pSymbolChanceConstant /= sum;
		}

		public MutationProfile(float mutabilityFactor) : this()
        {
			 pSymbolRemove    = pSymbolRemove    * mutabilityFactor;
			 pSymbolAdd       = pSymbolAdd       * mutabilityFactor;
			 pSymbolChanceNew = pSymbolChanceNew * mutabilityFactor; // this might make sense to leave out of this, but I think something with higher "mutability" should have a higher chance of creating a brand new symbol 
			 pAddSymbolBehind = pAddSymbolBehind * mutabilityFactor;
			 pBranchAdd       = pBranchAdd       * mutabilityFactor;
			 pBranchRemove    = pBranchRemove    * mutabilityFactor;
			 pLeafAdd         = pLeafAdd         * mutabilityFactor;
			 pLeafRemove      = pLeafRemove      * mutabilityFactor;

			 pRuleDuplicate   = pRuleDuplicate   * mutabilityFactor;
			 pRuleAdd         = pRuleAdd         * mutabilityFactor;
			 pRuleRemove      = pRuleRemove      * mutabilityFactor;

			 pAxiomMutate     = pAxiomMutate     * mutabilityFactor; 
        }
    }
}