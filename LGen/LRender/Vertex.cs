using System;
using System.Collections.Generic;

namespace LGen.LRender 
{ 
    public class VertexTree
    {
        public Vertex vertex;
        public List<VertexTree> children;

        public VertexTree(Vertex vertex)
        {
            this.vertex = vertex;
            this.children = new List<VertexTree>();
        }
    }

    public class Vertex
    {
        public float x;
        public float y;
        public float z;

        public float yaw;
        public float pitch;
        public float roll;

        public static Vertex Clone(Vertex original)
        {
            if (original == null) return null;

            Vertex v = new Vertex();
            
            v.x     = original.x;
            v.y     = original.y;
            v.z     = original.z;
            v.yaw   = original.yaw;
            v.pitch = original.pitch;
            v.roll  = original.roll;
            
            return v;
        }
    }
}