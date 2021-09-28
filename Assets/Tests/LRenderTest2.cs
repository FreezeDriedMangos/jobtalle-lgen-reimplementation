using LGen.LParse;
using LGen.LRender;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LRenderTest2 : MonoBehaviour
{
    
    public string s;
    public int num;

    public MeshRenderer leafRenderer;

    private Randomizer randomizer = new Randomizer();

    // Start is called before the first frame update
    void Start()
    {
        LGen.LRender.Renderer.initialize();
        LGen.LRender.Renderer r = new LGen.LRender.Renderer();
        AgentRenderData d = r.Render(s, randomizer, num);
        AgentRenderData d2 = r.Render(s, randomizer, num/2);
    }

    // Update is called once per frame
    void Update()
    {
        if (leafRenderer == null) return;

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        leafRenderer.GetPropertyBlock(propBlock);
                
        propBlock.SetInt("_Seed", randomizer.MakeInt_Inclusive(0, int.MaxValue-1));
        leafRenderer.SetPropertyBlock(propBlock);
    }
}
