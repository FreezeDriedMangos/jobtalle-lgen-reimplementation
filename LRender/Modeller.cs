using LGen.LParse;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LGen.LRender 
{ 
    public class Modeller
    {
        public List<Mesh> GenerateMeshes(Sentence sentence, float stemRadiusFactor = 0.01f)
        {
            VertexTree tree = GenerateTree(sentence);
            
            //List<int> stemLoad = new List<int>();
            Dictionary<VertexTree, int> vertexLoad = new Dictionary<VertexTree, int>();
            List<VertexTree[]> branches = new List<VertexTree[]>();

            makeBranches(branches, tree);
            calculateVertexLoad(vertexLoad, tree);

            List<Mesh> branchMeshes = new List<Mesh>();
            foreach(VertexTree[] branch in branches)
            {
                VertexTree lower = branch[0];
                VertexTree upper = branch[1];
                int topLoad = vertexLoad[upper];
                float topRadius = topLoad * stemRadiusFactor;
                float bottomRadius = (topLoad+1) * stemRadiusFactor;
                
                Vector3 start = new Vector3(lower.vertex.x, lower.vertex.y, lower.vertex.z);
                Vector3 end = new Vector3(upper.vertex.x, upper.vertex.y, upper.vertex.z);

                branchMeshes.Add(CreateCylinder(start, end, bottomRadius, topRadius));
            }

            return branchMeshes;
        }

        private void makeBranches(List<VertexTree[]> branches, VertexTree tree, VertexTree parent = null) 
        { 
            if (parent != null) branches.Add(new VertexTree[]{ parent, tree });

            foreach(VertexTree child in tree.children) makeBranches(branches, child, tree);
        } 

        private int calculateVertexLoad(Dictionary<VertexTree, int> result, VertexTree tree)
        {
            if (tree.children == null || tree.children.Count <= 0) 
            {
                result[tree] = 0;
                return 0;
            }

            int max = 0;
            foreach(VertexTree child in tree.children)
            {
                max = Math.Max(max, calculateVertexLoad(result, child));
            }

            result[tree] = max;
            return max;
        }

        public VertexTree GenerateTree(Sentence sentence, float branchLength = 1, float angleDelta = (float)(Math.PI/9f))
        {
            throw new NotImplementedException();

            Turtle turtle = new Turtle();
            Vertex currentLocation = turtle.GetVertexClone();
            VertexTree tree = new VertexTree(currentLocation);
            Stack<VertexTree> stack = new Stack<VertexTree>();

            VertexTree root = tree;

            foreach(Token t in sentence.Tokens)
            {
                if(t == Legend.BRANCH_OPEN)     { stack.Push(tree); }
                if(t == Legend.LEAF_OPEN)       { stack.Push(tree); }
                if(t == Legend.BRANCH_CLOSE)    { tree = stack.Pop(); turtle.location = tree.vertex; }
                if(t == Legend.ROLL_INCREMENT)  turtle.location.roll += angleDelta;
                if(t == Legend.ROLL_DECREMENT)  turtle.location.roll -= angleDelta;
                if(t == Legend.PITCH_INCREMENT) turtle.location.pitch += angleDelta;
                if(t == Legend.PITCH_DECREMENT) turtle.location.pitch -= angleDelta;
                if(t == Legend.YAW_INCREMENT)   turtle.location.yaw += angleDelta;
                if(t == Legend.YAW_DECREMENT)   turtle.location.yaw -= angleDelta;
                
                if(t.OnRangeInclusive(Legend.STEP_MIN, Legend.STEP_MAX)) 
                {
                    Vertex location = turtle.Forwards(branchLength);
                    VertexTree nextTree = new VertexTree(location);
                    
                    tree.children.Add(nextTree);
                    tree = nextTree;
                }
            }

            return root;
        }

        public Mesh CreateCylinder(Vector3 start, Vector3 end, float firstRadius, float secondRadius, int vertexCountPerEnd = 5)
        {
            Vector3 nxyz = end - start;

            List<Vector3> firstCircle = GenerateCircle
            (
                nxyz.x, 
                nxyz.y, 
                nxyz.z, 
                start.x, 
                start.y, 
                start.z,
                firstRadius,
                vertexCountPerEnd
            );

            List<Vector3> secondCircle = GenerateCircle
            (
                nxyz.x, 
                nxyz.y, 
                nxyz.z, 
                end.x, 
                end.y, 
                end.z,
                secondRadius,
                vertexCountPerEnd
            );

            List<Vector3> vertices = new List<Vector3>();
            vertices.AddRange(firstCircle);
            vertices.AddRange(secondCircle);
            
            List<int> triangles = new List<int>();
            for(int i = 0; i < vertexCountPerEnd; i++)
            {
                triangles.Add(i);
                triangles.Add((i+1)%vertexCountPerEnd);
                triangles.Add(i+vertexCountPerEnd);
            }
            for(int i = vertexCountPerEnd; i < 2*vertexCountPerEnd; i++)
            {
                triangles.Add(i);
                triangles.Add( (i+1) % (2*vertexCountPerEnd) );
                triangles.Add(i-vertexCountPerEnd);
            }

            List<Vector2> uvs = new List<Vector2>();
            for(int i = 0; i < vertexCountPerEnd; i++)
            {
                uvs.Add(new Vector2( 0, i/(vertexCountPerEnd-1) ));
            }
            for(int i = 0; i < vertexCountPerEnd; i++)
            {
                uvs.Add(new Vector2( 1, i/(vertexCountPerEnd-1) ));
            }

            List<Vector3> normals = new List<Vector3>();
            for(int i = 0; i < vertexCountPerEnd; i++)
            {
                normals.Add(vertices[i] - start);
            }
            for(int i = vertexCountPerEnd; i < 2*vertexCountPerEnd; i++)
            {
                normals.Add(vertices[i] - end);
            }

            Mesh mesh = new Mesh();
        
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.normals = normals.ToArray();

            return mesh;
        }

        private List<Vector3> GenerateCircle(float nx, float ny, float nz, float cx, float cy, float cz, float r, int numPoints)
        {
            // Code from https://stackoverflow.com/a/27715321/9643841
            // Only needed if normal vector (nx, ny, nz) is not already normalized.
            float s = 1.0f / (nx * nx + ny * ny + nz * nz);
            float v3x = s * nx;
            float v3y = s * ny;
            float v3z = s * nz;

            // Calculate v1.
            s = 1.0f / (v3x * v3x + v3z * v3z);
            float v1x = s * v3z;
            float v1y = 0.0f;
            float v1z = s * -v3x;

            // Calculate v2 as cross product of v3 and v1.
            // Since v1y is 0, it could be removed from the following calculations. Keeping it for consistency.
            float v2x = v3y * v1z - v3z * v1y;
            float v2y = v3z * v1x - v3x * v1z;
            float v2z = v3x * v1y - v3y * v1x;

            // For each circle point.
            List<Vector3> points = new List<Vector3>();
            float angleIncrement = 2*Mathf.PI / ((float)numPoints);
            for(float a = 0; a < 2*Mathf.PI; a += angleIncrement)
            {
                float px = cx + r * (v1x * Mathf.Cos(a) + v2x * Mathf.Sin(a));
                float py = cy + r * (v1y * Mathf.Cos(a) + v2y * Mathf.Sin(a));
                float pz = cz + r * (v1z * Mathf.Cos(a) + v2z * Mathf.Sin(a));
            }

            return points;
        }
    }
}