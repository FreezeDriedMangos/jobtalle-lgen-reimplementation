using LGen.LEvolve;
using LGen.LParse;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEvolveTest : MonoBehaviour
{
    public string axiom;
    public string[] rules;

    public int seed;

    public int numMutationSteps;
    public int numRuleApplicationSteps;

    // Start is called before the first frame update
    void Start()
    {
        LSystem s = new LSystem(axiom, rules);
        Mutator m = new Mutator();
        MutationProfile p = new MutationProfile();
        Randomizer r  = new Randomizer(seed);

        Debug.Log(s);

        for(int i = 0; i < numMutationSteps; i++) s = m.Mutate(s, p, r);
        Debug.Log(s);

        if (numRuleApplicationSteps > 0)
        {
            GrowthProfile g = new GrowthProfile.Quadratic(numRuleApplicationSteps);
            Sentence n = s.Generate(g, r);
            Debug.Log(n);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
