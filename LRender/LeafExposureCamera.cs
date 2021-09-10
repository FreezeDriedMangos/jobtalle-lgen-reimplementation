using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGen.LRender
{
    [RequireComponent(typeof(Camera))]
    public class LeafExposureCamera : MonoBehaviour
    {
        Camera camera;

        // Start is called before the first frame update
        void Start()
        {
            camera = GetComponent<Camera>();
            
            RenderTexture tex = Instantiate(RendererResources.Instance.LeafExposureRenderTexturePrefab); //new RenderTexture(RendererResources.Instance.LeafExposureTextureWidth, RendererResources.Instance.LeafExposureTextureHeight, 16);
            camera.targetTexture = tex;
            RendererResources.Instance.leafExposures.Add(tex);

            camera.cullingMask = 1 << LayerMask.NameToLayer("Plants"); //(1 << LayerMask.NameToLayer("TransparentFX")) | (1 << LayerMask.NameToLayer("OtherLayer"));
            camera.backgroundColor = Color.white;
            camera.clearFlags = CameraClearFlags.Color;
            camera.orthographic = true;
            camera.SetReplacementShader(RendererResources.Instance.LeafExposureShader, "");
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}