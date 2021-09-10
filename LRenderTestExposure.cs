using LGen.LParse;
using LGen.LRender;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LRenderTestExposure : MonoBehaviour
{
    public List<string> agents;
    public List<Vector3> agentPositions;


    LGen.LRender.Renderer r;
    List<AgentRenderData> agentData;

    // Start is called before the first frame update
    void Start()
    {
        agentData = new List<AgentRenderData>();
        r = new LGen.LRender.Renderer();
        Randomizer rand = new Randomizer();

        for(int i = 0; i < agents.Count; i++)
        {
            AgentRenderData d = r.Render(agents[i], rand, i);
            agentData.Add(d);
            d.gameObject.transform.position = agentPositions[i];
        }
        
    

        r.EvaluateExposure(agentData);
        List<float> exposures = agentData.ConvertAll(new System.Converter<AgentRenderData, float>((AgentRenderData d) => d.agentData.exposureReport.exposure));
        Debug.Log(exposures.Count);
        Debug.Log("[" + string.Join(", ", exposures) + "]");
    }

    bool done = false;

    // Update is called once per frame
    void Update()
    {
        if (done) return;
        done = true;


    }
}
