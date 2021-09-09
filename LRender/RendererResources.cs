using System.Collections.Generic;
using UnityEngine;

namespace LGen.LRender
{
    public class RendererResources : Singleton<RendererResources>
    {
        public Material leafExposureMaterial;
        public Material stemExposureMaterial;
        public Material seedExposureMaterial;

        public ComputeShader ComputeTotalLeafExposure;
        public List<RenderTexture> leafExposures;
    }
}