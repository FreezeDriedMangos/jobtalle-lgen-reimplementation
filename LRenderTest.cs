using LGen.LParse;
using LGen.LRender;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LRenderTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Modeller modeller = new Modeller();
        List<Mesh> branches = modeller.GenerateMeshes(new Sentence("AAA"));
        
        foreach(Mesh m in branches)
        {
            GameObject g = new GameObject();
            g.transform.parent = this.transform;
            
            MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = g.AddComponent<MeshFilter>();
            meshFilter.mesh = m;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
