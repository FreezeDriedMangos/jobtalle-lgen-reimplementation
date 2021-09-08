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
        public float removeSymbolChance = 0.01f;
        public float addBranchChance = 0.005f;
        public float removeBranchChance = 0.01f;
        public float addLeafChance = 0.005f;
        public float removeLeafChance = 0.01f;
        
        // meta stuff - these are only accessed while processing a mutation
        public float addSymbolBehindChance = 0.5f;
        public float useEChance = 0.8f;
    }
}