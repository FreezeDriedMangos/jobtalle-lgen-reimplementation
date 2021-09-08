namespace LGen.LEvolve
{
    public class MutationProfile
    {
        // misc
        public int sentenceSizeLimit = 10;
        public int NDopingAmount = 10;

        //
        // System level mutations
        //

        public float duplicateRuleChance = 0.01f;
        public float deleteRuleChance = 0.001f;
        public float createNewRuleChance = 0.001f;

        // meta
        public float chooseEForNewRuleLHSChance = 1f;

        //
        // Sentence-level mutations
        //

        // chances for each type of mutation to occur
        public float addSymbolChance = 0.02f;
        public float removeSymbolChance = 0.005f;
        public float addBranchChance = 0.005f;
        public float removeBranchChance = 0.0025f;
        public float addLeafChance = 0.005f;
        public float removeLeafChance = 0.0025f;
        
        // meta stuff - these are only accessed while processing a mutation
        public float addSymbolBehindChance = 0.5f;
        public float useEChance = 0.8f;
    }
}

// from Job Talle's repo:
// float pSymbolAdd = 0.005f,
// float pSymbolRemove = 0.005f,
// float pSymbolChanceNew = 0.2f,
// float pSymbolChanceRotation = 0.35f, // These probabilities are
// float pSymbolChanceSeed = 0.05f,     // relative to each other,
// float pSymbolChanceStep = 0.4f,      // their total probability
// float pSymbolChanceConstant = 0.2f,  // is 100%.
// float pSymbolChanceLeaf = 0.0f,      //
// float pBranchAdd = 0.002f,
// float pBranchRemove = 0.002f,
// float pLeafAdd = 0.002f,
// float pLeafRemove = 0.002f,
// float pRuleDuplicate = 0.003f,
// float pRuleAdd = 0.001f,
// float pRuleRemove = 0.004f