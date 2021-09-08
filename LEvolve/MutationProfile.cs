namespace LGen.LEvolve
{
    public class MutationProfile
    {
        // misc
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
		public float pBranchRemove = 0.002f;
		public float pLeafAdd = 0.002f;
		public float pLeafRemove = 0.002f;
		 
		public float pRuleDuplicate = 0.003f;
		public float pRuleAdd = 0.001f;
		public float pRuleRemove = 0.004f;

		public MutationProfile()
        {
			float sum = pSymbolChanceRotation + pSymbolChanceSeed + pSymbolChanceStep + pSymbolChanceConstant;
			pSymbolChanceRotation /= sum; 
			pSymbolChanceSeed 	  /= sum;
			pSymbolChanceStep 	  /= sum;
			pSymbolChanceConstant /= sum;
		}
    }
}