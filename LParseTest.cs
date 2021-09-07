using LGen.LParse;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LParseTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LSystem s = new LSystem
        (
            "A"+"\n"+
            "A => AB"+"\n"+
            "B => CA"
        );
    
        Debug.Log(s.Rules[0].LHS);
        Debug.Log(s.Rules[0].RHS);


        Sentence se = new Sentence("A");
        Debug.Log( se.Applicable(0, s.Rules[0]) );
        Debug.Log(se.Apply(s.Rules, new Randomizer(), 99));
        Debug.Log("Sentence " + se.ToString());


        Debug.Log(s.Generate(new GrowthProfile(3), new Randomizer()));
        Debug.Log(s.Generate(new GrowthProfile(3), new Randomizer()));
        Debug.Log(s.Generate(new GrowthProfile(3), new Randomizer()));
        Debug.Log(s.Generate(new GrowthProfile(3), new Randomizer()));

        //Sentence se = new Sentence("A");
        //Debug.Log( se.Apply(s.Rules, new Randomizer(), 99) );
        //Debug.Log("Sentence " + se.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
