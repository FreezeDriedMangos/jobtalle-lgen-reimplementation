using System;
using UnityEngine;

namespace LGen.LRender 
{ 

    public class Turtle
    {
        public Vertex location = new Vertex();

        public Vertex Forwards(float distance)
        {
            Vector3 dxyz = Utils.PolarToCartesian(distance, this.location.yaw, this.location.pitch);
            
            this.location.x += dxyz.x;
            this.location.y += dxyz.y;
            this.location.z += dxyz.z;

            return Vertex.Clone(this.location);
        }

        public Vertex GetVertexClone() { return Vertex.Clone(location); }

    }
}