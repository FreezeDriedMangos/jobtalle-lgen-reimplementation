
using LGen.LParse;
using System.Collections.Generic;
using UnityEngine;

namespace LGen.LRender
{
    public class AgentRenderData
    {
        public AgentData agentData;
        public GameObject gameObject;
        public Vector3Int leafIdentityColor;
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
            data.leafIdentityColor = new Vector3Int(agentNum % 256, agentNum / 256, 0);

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
                
                int leafR = data.leafIdentityColor.x;
                int leafG = data.leafIdentityColor.y;
                int leafB = 0; //i % 256;

                Color color = new Color((float)leafR / 256f, (float)leafG / 256f, (float)leafB / 256f);
                int seed = randomizer.MakeInt_Inclusive(0, int.MaxValue-1);

                propBlock.SetColor("_Color", color);
                propBlock.SetInt("_Seed", seed);
                propBlock.SetFloat("_Opacity", leafOpacity);
                meshRenderer.SetPropertyBlock(propBlock);
            }

            return data;
        }

        public void EvaluateExposure(List<AgentRenderData> agents)
        {
            // compute shader related code from https://en.wikibooks.org/wiki/Cg_Programming/Unity/Computing_Color_Histograms

            // count num occurances of each color using a compute shader
            ComputeShader shader = RendererResources.Instance.ComputeTotalLeafExposure;
            if (null == shader) 
            {
                Debug.Log("Shader or input texture missing.");
                return;
            }
         
            int handleInitialize = shader.FindKernel("HistogramInitialize");
            int handleMain = shader.FindKernel("HistogramMain");

            float numExposureTests = RendererResources.Instance.leafExposures.Count;
            foreach(RenderTexture leafExposure in RendererResources.Instance.leafExposures)
            {
                // count num occurances of each color using a compute shader
                if (null == leafExposure) 
                {
                    Debug.Log("Shader or input texture missing.");
                    return;
                }
                
                //
                // prepare to dispatch compute shader 
                //

                // note: max size of agents is 256*256
                ComputeBuffer histogramBuffer = new ComputeBuffer(256, sizeof(uint) * 4); // I really want agents.Count, sizeof(uint)
                uint[] histogramData = new uint[256 * 4]; // I need new uint[agents.Count]
      
                if (handleInitialize < 0 || handleMain < 0 || 
                    null == histogramBuffer || null == histogramData) 
                {
                    Debug.Log("Initialization failed.");
                    return;
                }

                shader.SetTexture(handleMain, "InputTexture", leafExposure);
                shader.SetBuffer(handleMain, "HistogramBuffer", histogramBuffer);
                shader.SetBuffer(handleInitialize, "HistogramBuffer", histogramBuffer);

                //
                // Dispatch compute shader
                //
                
                if (null == shader || null == leafExposure || 
                    0 > handleInitialize || 0 > handleMain ||
                    null == histogramBuffer || null == histogramData) 
                {
                    Debug.Log("Cannot compute histogram");
                    return;
                }
         
                shader.Dispatch(handleInitialize, 256 / 64, 1, 1);
                    // divided by 64 in x because of [numthreads(64,1,1)] in the compute shader code
                shader.Dispatch(handleMain, (leafExposure.width + 7) / 8, (leafExposure.height + 7) / 8, 1);
                    // divided by 8 in x and y because of [numthreads(8,8,1)] in the compute shader code
        
                histogramBuffer.GetData(histogramData);

                foreach(AgentRenderData agent in agents)
                {
                    int numPixels = 0; // histogramData[agent.leafIdentityColor.x + agent.leafIdentityColor.y*256]
                    agent.agentData.exposureReport.exposure += (float)numPixels/numExposureTests;
                }

                
                 histogramBuffer.Release();
                 histogramBuffer = null;
            }
        }
    }
}