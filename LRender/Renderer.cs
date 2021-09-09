
using LGen.LParse;
using UnityEngine;

namespace LGen.LRender
{
    public class AgentRenderData
    {
        public AgentData agentData;
        public GameObject gameObject;
    }

    public class Renderer
    {
        public AgentRenderData Render(Sentence sentence, Randomizer randomizer, int agentNum = 0)
        {
            Modeller m = new Modeller();
            return Render(m.GenerateAgentData(sentence), randomizer, agentNum);
        }

        public AgentRenderData Render(AgentData agent, Randomizer randomizer, int agentNum = 0, float leafOpacity = 0.8f)
        {
            AgentRenderData data = new AgentRenderData();
            data.agentData = agent;

            // non-leaves will get an unlit white shader so no need to do the above in the other loops

            GameObject go = new GameObject("Agent " + agentNum);
            data.gameObject = go;
        
            for(int i = 0; i < agent.meshes.stemMeshes.Count; i++)
            {
                Mesh m = agent.meshes.stemMeshes[i];
                GameObject g = new GameObject("stem " + i);
                g.transform.parent = go.transform;
            
                MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = g.AddComponent<MeshFilter>();
                meshFilter.mesh = m;
                meshRenderer.material = RendererResources.Instance.stemExposureMaterial;
            }

            for(int i = 0; i < agent.meshes.seedMeshes.Count; i++)
            {
                Mesh m = agent.meshes.seedMeshes[i];
                GameObject g = new GameObject("seed " + i);
                g.transform.parent = go.transform;
            
                MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = g.AddComponent<MeshFilter>();
                meshFilter.mesh = m;
                meshRenderer.material = RendererResources.Instance.seedExposureMaterial;
            }

            for(int i = 0; i < agent.meshes.leafMeshes.Count; i++)
            {
                Mesh m = agent.meshes.leafMeshes[i];
                GameObject g = new GameObject("leaf " + i);
                g.transform.parent = go.transform;
            
                MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = g.AddComponent<MeshFilter>();
                meshFilter.mesh = m;
                meshRenderer.material = RendererResources.Instance.leafExposureMaterial;

                // below code modified from https://thomasmountainborn.com/2016/05/25/materialpropertyblocks/
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(propBlock);
                
                int leafR = agentNum % 256;
                int leafG = agentNum / 256;
                int leafB = i % 256;
                Color color = new Color((float)leafR / 256f, (float)leafG / 256f, (float)leafB / 256f);
                Debug.Log(color);
                int seed = randomizer.MakeInt_Inclusive(0, int.MaxValue-1);
                Debug.Log(seed);
                propBlock.SetColor("_Color", color);
                propBlock.SetInt("_Seed", seed);
                propBlock.SetFloat("_Opacity", leafOpacity);
                meshRenderer.SetPropertyBlock(propBlock);
            }

            return data;
        }
    }
}