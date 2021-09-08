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



		public float pSymbolRemove = 0.005f;
		public float pSymbolAdd = 0.005f;
		public    float pSymbolChanceNew = 0.2f; // chance to use N instead of E
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

// from Job Talle's repo:
//
// float pSymbolAdd = 0.005f,
// float pSymbolRemove = 0.005f,
	// float pSymbolChanceNew = 0.2f, // chance to use N instead of E
	// float pSymbolChanceRotation = 0.35f, // These probabilities are
	// float pSymbolChanceSeed = 0.05f,     // relative to each other,
	// float pSymbolChanceStep = 0.4f,      // their total probability
	// float pSymbolChanceConstant = 0.2f,  // is 100%.
	// float pSymbolChanceLeaf = 0.0f,      //
// float pBranchAdd = 0.002f,
// float pBranchRemove = 0.002f,
// float pLeafAdd = 0.002f,
// float pLeafRemove = 0.002f,
//
// float pRuleDuplicate = 0.003f,
// float pRuleAdd = 0.001f,
// float pRuleRemove = 0.004f

/*
// N is never constructed
// the add symbol method doesn't work exactly like in the paper, it's different

LParse::Token Mutator::makeToken(LParse::Randomizer& randomizer) const {
	const auto chance = randomizer.makeFloat();

	if(chance <= pSymbolChanceRotation) {
		switch(randomizer.makeInt(0, 5)) {
		case 0:
			return LParse::Legend::PITCH_INCREMENT;
		case 1:
			return LParse::Legend::PITCH_DECREMENT;
		case 2:
			return LParse::Legend::ROLL_INCREMENT;
		case 3:
			return LParse::Legend::ROLL_DECREMENT;
		case 4:
			return LParse::Legend::YAW_INCREMENT;
		default:
			return LParse::Legend::YAW_DECREMENT;
		}
	}

	if(chance <= pSymbolChanceRotation + pSymbolChanceSeed)
		return LParse::Legend::SEED;

	if(chance <= pSymbolChanceRotation + pSymbolChanceSeed + pSymbolChanceStep)
		return randomizer.makeInt(LParse::Legend::STEP_MIN, LParse::Legend::STEP_MAX);

	// this is for single-symbol premade triangle leaves - I'm not implementing this
	if(chance <= pSymbolChanceRotation + pSymbolChanceSeed + pSymbolChanceStep + pSymbolChanceLeaf)
		return LParse::Legend::LEAF_A;

	return randomizer.makeInt(LParse::Legend::CONST_MIN, LParse::Legend::CONST_MAX);
}

LParse::Token Mutator::makeToken(LParse::Randomizer& randomizer, const GeneratedSymbols *constraints) const {
	if(!constraints || constraints->empty())
		return makeToken(randomizer);

	const auto chance = randomizer.makeFloat();

	if(chance <= constraints->getPRotation())
		return constraints->getRotations()[randomizer.makeInt(0, constraints->getRotations().size() - 1)];

	if(chance <= constraints->getPSeed() + constraints->getPRotation())
		return constraints->getSeeds()[randomizer.makeInt(0, constraints->getSeeds().size() - 1)];

	// if we're "using E", generate a random capital letter from E
	if(chance <= constraints->getPStep() + constraints->getPSeed() + constraints->getPRotation())
		return constraints->getSteps()[randomizer.makeInt(0, constraints->getSteps().size() - 1)];

	// this is for single-symbol premade triangle leaves - I'm not implementing this
	if(chance <= constraints->getPLeaf() + constraints->getPStep() + constraints->getPSeed() + constraints->getPRotation())
		return constraints->getLeaves()[randomizer.makeInt(0, constraints->getLeaves().size() - 1)];

	// if we're "using E", generate a random lower case letter from E
	return constraints->getConstants()[randomizer.makeInt(0, constraints->getConstants().size() - 1)];
}
*/