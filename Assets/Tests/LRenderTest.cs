using LGen.LParse;
using LGen.LRender;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LRenderTest : MonoBehaviour
{
    public Material branchMat;
    public Material seedMat;
    public Material leafMat;

    public string s;

    // Start is called before the first frame update
    void Start()
    {
        //string s = "A^^A++++A";
        //string s = "A^[AA^A^A]A_A____A^A";
        //string s = "<A^[AA^A^A]A_A____A^A]";
        //string s = "<A[^^B[^^C_C]_B_B]_A_A]";
        Modeller modeller = new Modeller();

        Vector3 v1 = new Vector3(1.0f, 2.7f, 0.0f);
        Vector3 v2 = new Vector3(1.0f, 3.7f, 0.0f);
        float rad = 0.1f;
        
        Vector3 n = v2-v1;
        //modeller.GenerateCircle(n.x, n.y, n.z, v1.x, v1.y, v1.z, rad, 5);
        //if(true) return;
        AgentData agent = Modeller.GenerateAgentData(new Sentence(s));
        
        foreach(Mesh m in agent.meshes.stemMeshes)
        {
            GameObject g = new GameObject("stem");
            g.transform.parent = this.transform;
            
            MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = g.AddComponent<MeshFilter>();
            meshFilter.mesh = m;
            meshRenderer.material = branchMat;
        }

        foreach(Mesh m in agent.meshes.seedMeshes)
        {
            GameObject g = new GameObject("seed");
            g.transform.parent = this.transform;
            
            MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = g.AddComponent<MeshFilter>();
            meshFilter.mesh = m;
            meshRenderer.material = seedMat;
        }

        foreach(Mesh m in agent.meshes.leafMeshes)
        {
            GameObject g = new GameObject("leaf");
            g.transform.parent = this.transform;
            
            MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = g.AddComponent<MeshFilter>();
            meshFilter.mesh = m;
            meshRenderer.material = leafMat;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
