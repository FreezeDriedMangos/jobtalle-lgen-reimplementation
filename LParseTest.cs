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
        
        Debug.Log( s.Generate(new GrowthProfile(3), new Randomizer()) );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
