using System.Collections.Generic;
using UnityEngine;

namespace LGen.LRender
{
    public class RendererResources : Singleton<RendererResources>
    {
        public Material leafExposureMaterial;
        public Material leafMaterial;
        public Material stemMaterial;
        public Material seedMaterial;


        public ComputeShader ComputeTotalLeafExposure;
        public List<RenderTexture> leafExposures = new List<RenderTexture>();
        public RenderTexture LeafExposureRenderTexturePrefab;
        public Shader ExposureShader;
        public List<Camera> leafExposureCameras = new List<Camera>();

        void Start()
        {
            Renderer.initialize();    
        }
    }
}