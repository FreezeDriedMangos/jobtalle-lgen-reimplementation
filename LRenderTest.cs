using LGen.LParse;
using LGen.LRender;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LRenderTest : MonoBehaviour
{
    public Material mat;

    // Start is called before the first frame update
    void Start()
    {
        //string s = "A^^A++++A";
        string s = "A^[AA^A^A]A_A____A^A";
        Modeller modeller = new Modeller();
        List<Mesh> branches = modeller.GenerateMeshes(new Sentence(s));
        
        foreach(Mesh m in branches)
        {
            GameObject g = new GameObject();
            g.transform.parent = this.transform;
            
            MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = g.AddComponent<MeshFilter>();
            meshFilter.mesh = m;
            meshRenderer.material = mat;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
