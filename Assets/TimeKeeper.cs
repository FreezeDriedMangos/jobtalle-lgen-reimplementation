using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeKeeper : Singleton<TimeKeeper>
{
    
    public string BuildSentences;
    public string RenderAgents;
    public string EvaluateLeafExposure;
    public string EvaluateViability;
    public string SeedNextState;
    public string SeedNextState_GenerateSeeds;
    public string SeedNextState_DistributeSeeds;
    public string SeedNextState_ResetSpecies;
    public string SeedNextState_RemoveOldAgents;

    //BuildAgentSentences(state);
    //RenderAgents_AndEvaluateLeafExposure(state);
    //EvaluateAgentsViability(state);            
    //SeedNextState

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
