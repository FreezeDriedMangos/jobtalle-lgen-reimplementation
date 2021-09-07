using LGen.LParse;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LGen.LRender 
{ 

    public class LeafArmature
    {
        public List<VertexTree> side1 = new List<VertexTree>();
        public List<VertexTree> side2 = new List<VertexTree>();
        public int leafDepth;
        public bool active = true;

        public LeafArmature(int leafDepth)
        {
            this.leafDepth = leafDepth;
        }

        public LeafArmature(VertexTree tree, int leafDepth) : this(leafDepth)
        {
            this.Add(tree, leafDepth);
        }

        public void Add(VertexTree vertexTree, int leafDepth)
        {
            if (!active) return;

            if (leafDepth == this.leafDepth) side1.Add(vertexTree);
            else if (leafDepth == this.leafDepth-1) side2.Add(vertexTree);
        }

        public void SetActive(int leafDepth) { if (active && leafDepth <= this.leafDepth-2) active = false; }
    }
    public class Armature
    {
        public VertexTree VertexTree;
        public List<LeafArmature> leaves;
        public List<Vector3> seeds = new List<Vector3>();
    }

    public class Modeller
    {
        public List<Mesh> GenerateMeshes(Sentence sentence, float stemRadiusFactor = 0.05f, float seedSize = 0.2f)
        {
            Armature armature = GenerateTree(sentence);
            VertexTree tree = armature.VertexTree;
            
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

            List<Mesh> leafMeshes = new List<Mesh>();
            foreach (LeafArmature leaf in armature.leaves)
            {
                List<Vector3> vertices = new List<Vector3>();
                for(int i = 0; i < leaf.side1.Count; i++)
                {
                    Vertex v = leaf.side1[i].vertex;
                    vertices.Add(new Vector3(v.x, v.y, v.z));
                }
                for(int i = leaf.side2.Count-1; i >= 0; i--)
                {
                    Vertex v = leaf.side2[i].vertex;
                    vertices.Add(new Vector3(v.x, v.y, v.z));
                }
                Debug.Log("leaf vertex count: " + vertices.Count);
                
                List<int> triangles = new List<int>();
                // nice, but doesn't work for concave leaves
                //for (int i = 1; i < vertices.Count-1; i++)
                //{
                //    triangles.Add(i);
                //    triangles.Add(i+1);
                //    triangles.Add(0);
                    
                //    triangles.Add(i+1);
                //    triangles.Add(i);
                //    triangles.Add(0);
                //}
                // verticies now contains all verticies of the leaf, in clockwise order
                Vector3 center = Vector3.zero;
                foreach(Vector3 vertex in vertices) center += vertex;
                center /= (float)vertices.Count;
                vertices.Add(center);
                int centerIdx = vertices.Count-1;

                for (int i = 0; i < vertices.Count-1; i++)
                {
                    triangles.Add(i);
                    triangles.Add((i+1)%(vertices.Count-1));
                    triangles.Add(centerIdx);

                    triangles.Add((i+1)%(vertices.Count-1));
                    triangles.Add(i);
                    triangles.Add(centerIdx);

                }

                List<Vector2> uvs = new List<Vector2>();
                for (int i = 0; i < vertices.Count; i++) uvs.Add(new Vector2());

                
                Mesh mesh = new Mesh();
        
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.uv = uvs.ToArray();
                //mesh.normals = normals.ToArray();
                mesh.RecalculateNormals();

                leafMeshes.Add(mesh);
            }

            List<Mesh> seedMeshes = new List<Mesh>();
            foreach(Vector3 seedCenter in armature.seeds)
            {
                seedMeshes.Add(CreateIcosphere.Create(seedSize, seedCenter));
            }

            List<Mesh> meshes = new List<Mesh>();
            meshes.AddRange(branchMeshes);
            meshes.AddRange(leafMeshes);
            meshes.AddRange(seedMeshes);
            return meshes;
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
                max = Math.Max(max, 1+calculateVertexLoad(result, child));
            }

            result[tree] = max;
            return max;
        }

        public Armature GenerateTree(Sentence sentence, float branchLength = 1, float angleDelta = (float)(Math.PI/9f))
        {
            Armature armature = new Armature();

            Turtle turtle = new Turtle();
            Vertex currentLocation = turtle.GetVertexClone();
            VertexTree tree = new VertexTree(currentLocation);
            Stack<VertexTree> stack = new Stack<VertexTree>();
            Stack<Vertex> turtleStack = new Stack<Vertex>();

            armature.VertexTree = tree;

            List<LeafArmature> leaves = new List<LeafArmature>();
            
            armature.leaves = leaves;

            // points only get added to leaves if they're on the same leaf depth the given leaf started on, or one shallower
            // a leaf permanently ends if a leaf depth of startingDepth-2 is reached

            Stack<LeafArmature> leafStack = new Stack<LeafArmature>();
            int leafDepth = 0;

            foreach(Token t in sentence.Tokens)
            {
                //
                // misc
                //
                if (t == Legend.SEED) armature.seeds.Add(new Vector3(turtle.location.x, turtle.location.y, turtle.location.z));

                //
                // branches and leaves
                //
                if(t == Legend.BRANCH_OPEN)     
                { 
                    stack.Push(tree); 
                    turtleStack.Push(turtle.GetVertexClone());
                    
                    if(leafDepth > 0) 
                    {
                        leafDepth++;
                        leaves.Add(new LeafArmature(tree, leafDepth));
                    }
                }
                if(t == Legend.LEAF_OPEN)       
                { 
                    stack.Push(tree);
                    turtleStack.Push(turtle.GetVertexClone());

                    leafDepth++;
                    //leaves.Add(new LeafArmature(tree, leafDepth));
                }
                if(t == Legend.BRANCH_CLOSE)    
                { 
                    tree = stack.Pop(); 
                    turtle.location = turtleStack.Pop();
                    
                    if (leafDepth > 0)
                    {
                        leafDepth--;
                        
                        foreach(LeafArmature leaf in leaves) leaf.SetActive(leafDepth);
                    }
                }

                //
                // change direction
                //
                if(t == Legend.ROLL_INCREMENT)  turtle.location.roll += angleDelta;
                if(t == Legend.ROLL_DECREMENT)  turtle.location.roll -= angleDelta;
                if(t == Legend.PITCH_INCREMENT) turtle.location.pitch += angleDelta;
                if(t == Legend.PITCH_DECREMENT) turtle.location.pitch -= angleDelta;
                if(t == Legend.YAW_INCREMENT)   turtle.location.yaw += angleDelta;
                if(t == Legend.YAW_DECREMENT)   turtle.location.yaw -= angleDelta;
                
                //
                // move forward
                //
                if(t.OnRangeInclusive(Legend.STEP_MIN, Legend.STEP_MAX)) 
                {
                    Vertex location = turtle.Forwards(branchLength);
                    VertexTree nextTree = new VertexTree(location);
                    
                    tree.children.Add(nextTree);
                    tree = nextTree;
                    
                    //if (workingLeaf1 != null) workingLeaf1.Add(nextTree, leafDepth);
                    //if (workingLeaf2 != null) workingLeaf2.Add(nextTree, leafDepth);
                    foreach(LeafArmature leaf in leaves)
                    {
                        leaf.Add(nextTree, leafDepth);
                    }
                }

                // update current position after rotating
                // tree.vertex = Vertex.Clone(turtle.location);
            }

            return armature;
        }

        public Mesh CreateCylinder(Vector3 start, Vector3 end, float firstRadius, float secondRadius, int vertexCountPerEnd = 5)
        {
            firstRadius = Mathf.Max(firstRadius, 0.001f);
            secondRadius = Mathf.Max(secondRadius, 0.001f);
            
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
                int circleIdx1 = i;
                int circleIdx2 = (i+1)%vertexCountPerEnd;
                triangles.Add(circleIdx1);
                triangles.Add(circleIdx2);
                triangles.Add(circleIdx1+vertexCountPerEnd);

                triangles.Add(circleIdx1+vertexCountPerEnd);
                triangles.Add(circleIdx2);
                triangles.Add(circleIdx2+vertexCountPerEnd);
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
                points.Add(new Vector3(px, py, pz));
            }

            return points;
        }
    }
}