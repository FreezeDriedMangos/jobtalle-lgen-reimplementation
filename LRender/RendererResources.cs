﻿using System.Collections.Generic;
using UnityEngine;

namespace LGen.LRender
{
    public class RendererResources : Singleton<RendererResources>
    {
        public Material leafExposureMaterial;
        //public Material stemExposureMaterial;
        //public Material seedExposureMaterial;
        public Material leafMaterial;
        public Material stemMaterial;
        public Material seedMaterial;


        public ComputeShader ComputeTotalLeafExposure;
        public List<RenderTexture> leafExposures = new List<RenderTexture>();
        public RenderTexture LeafExposureRenderTexturePrefab;
        public Shader LeafExposureShader;
        public Texture testTex;
        public List<Camera> leafExposureCameras = new List<Camera>();
    }
}