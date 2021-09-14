﻿
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
        public List<AgentRenderData> Render(List<Sentence> sentences, Randomizer randomizer, Transform parent = null, float leafOpacity = 0.8f, List<float> fertilities = null, float maxExpectedBranchLoad = 2f, float stemRadiusFactor = 0.02f, float seedSize = 0.2f, float branchLength = 0.5f, float angleDelta = (float)(Mathf.PI/9f), float seedOffset = 0/*0.2f*/)
        {
            List<AgentRenderData> agentRenderData = new List<AgentRenderData>();
            List<AgentData> agentData = new List<AgentData>();    

            for(int i = 0; i < sentences.Count; i++)
            {
                agentData.Add(Modeller.GenerateAgentData(sentences[i], stemRadiusFactor, seedSize, branchLength, angleDelta, seedOffset));
            }

            for(int i = 0; i < sentences.Count; i++)
            {
                agentRenderData.Add(Render(agentData[i], randomizer, i, parent, leafOpacity, (fertilities == null || fertilities.Count <= i) ? 1f : fertilities[i], maxExpectedBranchLoad));
            }

            return agentRenderData;
        }

        public AgentRenderData Render(Sentence sentence, Randomizer randomizer, int agentNum = 0, Transform parent = null, float leafOpacity = 0.8f, float fertility = 1f, float maxExpectedBranchLoad = 2f, float stemRadiusFactor = 0.02f, float seedSize = 0.2f, float branchLength = 0.5f, float angleDelta = (float)(Mathf.PI/9f), float seedOffset = 0/*0.2f*/)
        {
            return Render(Modeller.GenerateAgentData(sentence, stemRadiusFactor, seedSize, branchLength, angleDelta, seedOffset), randomizer, agentNum, parent, leafOpacity, fertility, maxExpectedBranchLoad);
        }

        public AgentRenderData Render(AgentData agent, Randomizer randomizer, int agentNum = 0, Transform parent = null, float leafOpacity = 0.8f, float fertility = 1f, float maxExpectedBranchLoad = 2f)
        {
            AgentRenderData data = new AgentRenderData();
            data.agentData = agent;
            data.leafIdentityColor = new Vector3Int(agentNum % 256, agentNum / 256, 0);

            // non-leaves will get an unlit white shader so no need to do the above in the other loops

            GameObject go = new GameObject("Agent " + agentNum);
            go.transform.parent = parent;
            go.layer = LayerMask.NameToLayer("Plants");
            data.gameObject = go;
        
            for(int i = 0; i < agent.meshes.stemMeshes.Count; i++)
            {
                Mesh m = agent.meshes.stemMeshes[i];
                GameObject g = new GameObject("stem " + i);
                g.transform.parent = go.transform;
                g.layer = LayerMask.NameToLayer("Plants");

                MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = g.AddComponent<MeshFilter>();
                meshFilter.mesh = m;
                meshRenderer.material = RendererResources.Instance.stemMaterial; //leafExposureMaterial;
        
                // below code modified from https://thomasmountainborn.com/2016/05/25/materialpropertyblocks/
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Fertility", fertility);
                propBlock.SetFloat("_StemLoad", agent.branchReports[i].branchLoad / maxExpectedBranchLoad);
                propBlock.SetColor("_LeafExposureColor", Color.white);
                propBlock.SetInt("_Seed", 0);
                propBlock.SetFloat("_Opacity", 1);
                meshRenderer.SetPropertyBlock(propBlock);
            }

            for(int i = 0; i < agent.meshes.seedMeshes.Count; i++)
            {
                Mesh m = agent.meshes.seedMeshes[i];
                GameObject g = new GameObject("seed " + i);
                g.transform.parent = go.transform;
                g.layer = LayerMask.NameToLayer("Plants");
            
                MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = g.AddComponent<MeshFilter>();
                meshFilter.mesh = m;
                meshRenderer.material = RendererResources.Instance.seedMaterial; //leafExposureMaterial;
        
                // below code modified from https://thomasmountainborn.com/2016/05/25/materialpropertyblocks/
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(propBlock);
                propBlock.SetFloat("_Fertility", fertility);
                propBlock.SetColor("_LeafExposureColor", Color.white);
                propBlock.SetInt("_Seed", 0);
                propBlock.SetFloat("_Opacity", 1);
                meshRenderer.SetPropertyBlock(propBlock);
            }

            for(int i = 0; i < agent.meshes.leafMeshes.Count; i++)
            {
                Mesh m = agent.meshes.leafMeshes[i];
                GameObject g = new GameObject("leaf " + i);
                g.transform.parent = go.transform;
                g.layer = LayerMask.NameToLayer("Plants");
            
                MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = g.AddComponent<MeshFilter>();
                meshFilter.mesh = m;
                meshRenderer.material = RendererResources.Instance.leafMaterial; //leafExposureMaterial;

                // below code modified from https://thomasmountainborn.com/2016/05/25/materialpropertyblocks/
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                meshRenderer.GetPropertyBlock(propBlock);
                
                int leafR = data.leafIdentityColor.x;
                int leafG = data.leafIdentityColor.y;
                int leafB = 0; //i % 256;

                Color color = new Color((float)leafR / 256f, (float)leafG / 256f, (float)leafB / 256f);
                int seed = randomizer.MakeInt_Inclusive(0, int.MaxValue-1);
        
                propBlock.SetFloat("_Fertility", fertility);
                propBlock.SetColor("_LeafExposureColor", color);
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
            //foreach(RenderTexture leafExposure in RendererResources.Instance.leafExposures)
            foreach(Camera leafExposureCamera in RendererResources.Instance.leafExposureCameras)
            {
                Texture2D leafExposure = RTImage(leafExposureCamera);

                // count num occurances of each color using a compute shader
                if (null == leafExposureCamera) 
                {
                    Debug.Log("Shader or input texture missing.");
                    return;
                }
                
                //
                // prepare to dispatch compute shader 
                //

                // note: everywhere you see 64*agents64, pretend it says agents.count
                // I just needed the first multiple of 64 greather than or equal to agents.Count for this all to work nicely
                int agents64 = (int)Mathf.Ceil((float)agents.Count / 64f);

                // note: max size of agents is 256*256
                ComputeBuffer histogramBuffer = new ComputeBuffer(64*agents64, sizeof(uint)); //new ComputeBuffer(256, sizeof(uint) * 4); // I really want agents.Count, sizeof(uint)
                uint[] histogramData = new uint[64*agents64]; //new uint[256 * 4]; // I need new uint[agents.Count]
      
                if (handleInitialize < 0 || handleMain < 0 || 
                    null == histogramBuffer || null == histogramData) 
                {
                    Debug.Log("Initialization failed.");
                    return;
                }

                shader.SetTexture(handleMain, "InputTexture", leafExposure);//RendererResources.Instance.testTex);//leafExposure);
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
         
                //shader.Dispatch(handleInitialize, 256 / 64, 1, 1);
                shader.Dispatch(handleInitialize, agents64, 1, 1);
                //    // divided by 64 in x because of [numthreads(64,1,1)] in the compute shader code
                //shader.Dispatch(handleInitialize, (leafExposure.width + 7) / 8, (leafExposure.height + 7) / 8, 1);
                shader.Dispatch(handleMain, (leafExposure.width + 7) / 8, (leafExposure.height + 7) / 8, 1);
                    // divided by 8 in x and y because of [numthreads(8,8,1)] in the compute shader code
        
                histogramBuffer.GetData(histogramData);

                foreach(AgentRenderData agent in agents)
                {
                    uint numPixels = histogramData[agent.leafIdentityColor.x + agent.leafIdentityColor.y*256]; //0; // histogramData[agent.leafIdentityColor.x + agent.leafIdentityColor.y*256]
                    agent.agentData.exposureReport.exposure += (float)numPixels/numExposureTests;
                }

                
                 histogramBuffer.Release();
                 histogramBuffer = null;
            }
        }

        Texture2D RTImage(Camera camera)
        {
            // The Render Texture in RenderTexture.active is the one
            // that will be read by ReadPixels.
            var currentRT = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;

            // Render the camera's view.
            camera.Render();

            // Make a new texture and read the active Render Texture into it.
            Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
            image.filterMode = FilterMode.Point;    
            image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            image.Apply();
            
            // Replace the original active Render Texture.
            RenderTexture.active = currentRT;
            return image;
        }
    }
}