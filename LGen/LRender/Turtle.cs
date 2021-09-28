using System;
using UnityEngine;

namespace LGen.LRender 
{ 

    public class Turtle
    {
        public Vertex location = new Vertex();

        public Turtle()
        {
            location.pitch = -Mathf.PI/2;
        }

        public Vertex Forwards(float distance)
        {
            Vector3 dxyz = distance * Utils.ApplyRollPitchYaw(location.roll, location.pitch, location.yaw, Vector3.forward);

            this.location.x += dxyz.x;
            this.location.y += dxyz.y;
            this.location.z += dxyz.z;

            return Vertex.Clone(this.location);
        }

        public Vertex GetVertexClone() { return Vertex.Clone(location); }

        public Vector3 GetSideways(float distance)
        {
            Vector3 dxyz = distance * Utils.ApplyRollPitchYaw(location.roll, location.pitch, location.yaw, Vector3.right);
            
            Vertex v = GetVertexClone();
            
            return new Vector3(
                this.location.x + dxyz.x,
                this.location.y + dxyz.y,
                this.location.z + dxyz.z
            );
        }

    }
}