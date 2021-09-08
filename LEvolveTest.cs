﻿using LGen.LEvolve;
using LGen.LParse;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LEvolveTest : MonoBehaviour
{
    public string axiom;
    public string[] rules;

    public int seed;

    public int parseCount;

    // Start is called before the first frame update
    void Start()
    {
        LSystem s = new LSystem(axiom, rules);
        Mutator m = new Mutator();
        MutationProfile p = new MutationProfile();
        Randomizer r  = new Randomizer(seed);

        Debug.Log(s);
        
        s = m.Mutate(s, p, r);
        Debug.Log(s);
        
        for(int i = 0; i < 100; i++) s = m.Mutate(s, p, r);
        Debug.Log(s);

        if (parseCount > 0)
        {
            GrowthProfile g = new GrowthProfile.Quadratic(parseCount);
            Sentence n = s.Generate(g, r);
            Debug.Log(n);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
