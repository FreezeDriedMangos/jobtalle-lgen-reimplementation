using System;
using UnityEngine;

namespace LGen.LRender 
{ 

    public class Turtle
    {
        public Vertex location = new Vertex();

        public Vertex Forwards(float distance)
        {
            Debug.Log(this.location.yaw + ", " + this.location.pitch);
            Vector3 dxyz = Utils.PolarToCartesian(distance, this.location.yaw, this.location.pitch);
            Debug.Log(dxyz);
            
            this.location.x += dxyz.x;
            this.location.y += dxyz.y;
            this.location.z += dxyz.z;

            return Vertex.Clone(this.location);
        }

        public Vertex GetVertexClone() { return Vertex.Clone(location); }

    }
}