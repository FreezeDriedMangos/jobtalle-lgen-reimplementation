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

        public Sentence sentence;
    }

    public class Modeller
    {
        public static Mesh unitCylinder = CreateCylinder(0, 0, 0, new Vector3(0, 0, -1), Vector3.zero, 1, 1).CreateMesh();
        public static Mesh unitIcosahedron = CreateIcosphere.Create(1, Vector3.zero, 0);

        public static AgentData GenerateAgentData(Sentence sentence, float stemRadiusFactor = 0.02f, float seedSize = 0.1f, float branchLength = 0.5f, float angleDelta = (float)(Math.PI/9f), float seedOffset = 0/*0.2f*/)
        {
            AgentData agent = new AgentData();
            Armature armature = GenerateTree(sentence, branchLength, angleDelta, seedOffset);
            armature.sentence = sentence;
        
            agent.positionReport.centerOfGravity = CalculateCenterOfGravity(armature.VertexTree);
            agent.limitsReport = CalculateLimitsReport(armature.VertexTree, agent.limitsReport);    
            agent.seedReports = armature.seeds.ConvertAll(new Converter<Vector3, SeedReport>((Vector3 v) => new SeedReport(v) ));

            agent.meshes = GenerateMeshes(armature, agent.branchReports, stemRadiusFactor, seedSize);

            foreach(MeshData m in agent.meshes.uncompiledLeafMeshes)
            {
                LeafReport r = new LeafReport();
                agent.leafReports.Add(r);
                
                for(int i = 0; i < m.triangles.Length; i += 6)
                {
                    int t0 = m.triangles[i+0];
                    int t1 = m.triangles[i+1];
                    int t2 = m.triangles[i+2];
                    
                    Vector3 A = m.vertices[t0];
                    Vector3 B = m.vertices[t0];
                    Vector3 C = m.vertices[t0];

                    // area of a 3D triangle formula from https://math.stackexchange.com/a/128999
                    Vector3 AB = B - A;
                    Vector3 AC = C - A;

                    r.area += 0.5f * Vector3.Cross(AB, AC).magnitude;
                }
            }

            return agent;
        }

        public static AgentMeshes GenerateMeshes(Armature armature, List<BranchReport> branchReports, float stemRadiusFactor = 0.02f, float seedSize = 0.2f)
        {
            AgentMeshes agent = new AgentMeshes();

            Dictionary<VertexTree, int> vertexLoad = new Dictionary<VertexTree, int>();
            List<VertexTree[]> branches = new List<VertexTree[]>();

            VertexTree tree = armature.VertexTree;

            makeBranches(branches, tree);
            calculateVertexLoad(vertexLoad, tree);
            
            List<MeshData> branchMeshes = new List<MeshData>();
            foreach(VertexTree[] branch in branches)
            {
                VertexTree lower = branch[0];
                VertexTree upper = branch[1];
                int topLoad = vertexLoad[upper];
                float topRadius = topLoad * stemRadiusFactor;
                float bottomRadius = (topLoad+1) * stemRadiusFactor;
                
                Vector3 start = new Vector3(lower.vertex.x, lower.vertex.y, lower.vertex.z);
                Vector3 end = new Vector3(upper.vertex.x, upper.vertex.y, upper.vertex.z);
                
                //UnityEngine.Debug.Log(start + " to " + end + " - " + bottomRadius + ", " + topRadius);
                MeshData meshData = new MeshData(); //CreateCylinder(upper.vertex.roll, upper.vertex.pitch, upper.vertex.yaw, start, end, bottomRadius, topRadius);
                branchMeshes.Add(meshData);
                
                meshData.branch_topRadius = topRadius;
                meshData.branch_bottomRadius = bottomRadius;
                meshData.branch_rotation = Utils.GetQuaternion(upper.vertex.roll, upper.vertex.pitch, upper.vertex.yaw);
                meshData.branch_topLocation = new Vector3(upper.vertex.x, upper.vertex.y, upper.vertex.z);
                meshData.branch_length = Vector3.Distance(start, end);

                BranchReport b = new BranchReport();
                b.branchLoad = topLoad;
                branchReports.Add(b);
            }
            
            List<MeshData> seedMeshes = new List<MeshData>();
            foreach(Vector3 seedCenter in armature.seeds)
            {
                //seedMeshes.Add(CreateCube(seedSize, seedCenter));//CreateIcosphere.Create(seedSize, seedCenter));

                MeshData data = new MeshData();
                data.seed_size = seedSize;
                data.seed_location = seedCenter;
                seedMeshes.Add(data);
            }

            List<MeshData> leafMeshes = new List<MeshData>();
            foreach (LeafArmature leaf in armature.leaves)
            {
                if (leaf.side1.Count == 0 && leaf.side2.Count == 0) continue; 
                
                if (leaf.side2.Count == 0) leaf.side2.Add(leaf.side1[0]);
                if (leaf.side1.Count == 0) leaf.side2.Add(leaf.side2[0]);

                List<Vector3> vertices = new List<Vector3>();
                for(int i = 0; i < leaf.side1.Count; i++)
                {
                    Vertex v = leaf.side1[i].vertex;
                    vertices.Add(new Vector3(v.x, v.y, v.z));
                }
                for(int i = 0; i < leaf.side2.Count; i++)
                {
                    Vertex v = leaf.side2[i].vertex;
                    vertices.Add(new Vector3(v.x, v.y, v.z));
                }

                if (vertices.Count <= 2) continue;

                vertices.AddRange(vertices); // duplicate every entry to the vertices array so that the front and back sides of the leaf don't share vertices

                // Strategy for generating leaf meshes
                // zigzag between the two branches that make up the leaf, like a spider making a web

                List<int> triangles = new List<int>();
                
                int N = Mathf.Max(leaf.side1.Count, leaf.side2.Count);
                int s1 = 0;
                int s2 = leaf.side1.Count;
                int backSide = vertices.Count/2;
                int prev = -1;
                int curr = 0+s1;
                int next = 0+s2;
                for(int i = 2; i < 2*N; i++)
                {
                    prev = curr;
                    curr = next;
                    next = i%2 == 0? Mathf.Min(i/2, leaf.side1.Count-1)+s1 : Mathf.Min(i/2, leaf.side2.Count-1)+s2;

                    if (i%2 == 0)
                    {
                        triangles.Add(prev);
                        triangles.Add(next);
                        triangles.Add(curr);
        
                        triangles.Add(prev+backSide);
                        triangles.Add(curr+backSide);
                        triangles.Add(next+backSide);
                    }
                    else
                    {
                        triangles.Add(prev);
                        triangles.Add(curr);
                        triangles.Add(next);
        
                        triangles.Add(prev+backSide);
                        triangles.Add(next+backSide);
                        triangles.Add(curr+backSide);
                    }
                }

                List<Vector2> uvs = new List<Vector2>();
                for (int i = 0; i < vertices.Count; i++) uvs.Add(new Vector2());

                
                //Mesh mesh = new Mesh();
                
                //mesh.vertices = vertices.ToArray();
                //mesh.triangles = triangles.ToArray();
                //mesh.uv = uvs.ToArray();
                ////mesh.normals = normals.ToArray();
                //mesh.RecalculateNormals();
                //mesh.RecalculateBounds();
                //mesh.RecalculateTangents();

                leafMeshes.Add(new MeshData() { vertices = vertices.ToArray(), triangles = triangles.ToArray(), uvs = uvs.ToArray() });
            }

            agent.uncompiledStemMeshes = branchMeshes;
            agent.uncompiledSeedMeshes = seedMeshes;
            agent.uncompiledLeafMeshes = leafMeshes;

            return agent;
        }

        private static Vector3 CalculateCenterOfGravity(VertexTree tree, bool topLevel = true)
        {
            Vector3 v = new Vector3(tree.vertex.x, tree.vertex.y, tree.vertex.z);
            foreach(VertexTree child in tree.children) v += CalculateCenterOfGravity(child, false);
            return topLevel? v/((float)CountVertices(tree)) : v;
        }
        private static int CountVertices(VertexTree tree)
        {
            int n = 1;
            foreach(VertexTree child in tree.children) n += CountVertices(child);
            return n;
        }
        private static LimitsReport CalculateLimitsReport(VertexTree tree, LimitsReport report)
        {
            report.Add(new Vector3(tree.vertex.x, tree.vertex.y, tree.vertex.z));
            foreach(VertexTree child in tree.children) CalculateLimitsReport(child, report);
            return report;
        }

        private static void makeBranches(List<VertexTree[]> branches, VertexTree tree, VertexTree parent = null) 
        { 
            if (parent != null) branches.Add(new VertexTree[]{ parent, tree });

            foreach(VertexTree child in tree.children) makeBranches(branches, child, tree);
        } 

        private static int calculateVertexLoad(Dictionary<VertexTree, int> result, VertexTree tree)
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

        public static Armature GenerateTree(Sentence sentence, float branchLength = 0.5f, float angleDelta = (float)(Math.PI/9f), float seedOffset = 0/*0.2f*/)
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

                // TODO: add seed to location turtle.Sideways, which uses roll
                // this way seeds aren't embedded in the stem
                //if (t == Legend.SEED) armature.seeds.Add(new Vector3(turtle.location.x, turtle.location.y, turtle.location.z));
                if (t == Legend.SEED) armature.seeds.Add(turtle.GetSideways(seedOffset));

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
                    
                    foreach(LeafArmature leaf in leaves)
                    {
                        leaf.Add(nextTree, leafDepth);
                    }
                }
            }

            return armature;
        }

        public static MeshData CreateCylinder(float roll, float pitch, float yaw, Vector3 start, Vector3 end, float firstRadius, float secondRadius, int vertexCountPerEnd = 5)
        {
            firstRadius = Mathf.Max(firstRadius, 0.001f);
            secondRadius = Mathf.Max(secondRadius, 0.001f);
            
            Vector3 nxyz = end - start;

            List<Vector3> firstCircle = GenerateCircle(roll, pitch, yaw, start, firstRadius, vertexCountPerEnd);
            List<Vector3> secondCircle = GenerateCircle(roll, pitch, yaw, end, secondRadius, vertexCountPerEnd);


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

            //Mesh mesh = new Mesh();
        
            //mesh.vertices = vertices.ToArray();
            //mesh.triangles = triangles.ToArray();
            //mesh.uv = uvs.ToArray();
            //mesh.normals = normals.ToArray();

            //return mesh;

            return new MeshData() { vertices = vertices.ToArray(), triangles = triangles.ToArray(), uvs = uvs.ToArray() };
        }

        public static List<Vector3> GenerateCircle(float roll, float pitch, float yaw, Vector3 center, float r, int numPoints)
        {
            List<Vector3> points = new List<Vector3>();
            float angleIncrement = 2*Mathf.PI / ((float)numPoints);
            for(float a = 0; a < 2*Mathf.PI; a += angleIncrement)
            {
                float px = r * Mathf.Cos(a);
                float py = r * Mathf.Sin(a);
                float pz = r * 0;
                Vector3 basePoint = new Vector3(px, py, pz);
                Vector3 rotatedPoint = Utils.ApplyRollPitchYaw(roll, pitch, yaw, basePoint);
                Vector3 finalPoint = rotatedPoint + center;
                points.Add(finalPoint);
            }

            return points;
        }

        private static MeshData CreateCube (float radius, Vector3 center) {
            Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);
            // function modified from http://ilkinulas.github.io/development/unity/2016/04/30/cube-mesh-in-unity3d.html
		    Vector3[] vertices = {
			    (new Vector3 (0, 0, 0) - offset)*radius + center,
			    (new Vector3 (1, 0, 0) - offset)*radius + center,
			    (new Vector3 (1, 1, 0) - offset)*radius + center,
			    (new Vector3 (0, 1, 0) - offset)*radius + center,
			    (new Vector3 (0, 1, 1) - offset)*radius + center,
			    (new Vector3 (1, 1, 1) - offset)*radius + center,
			    (new Vector3 (1, 0, 1) - offset)*radius + center,
			    (new Vector3 (0, 0, 1) - offset)*radius + center,
		    };

		    int[] triangles = {
			    0, 2, 1, //face front
			    0, 3, 2,
			    2, 3, 4, //face top
			    2, 4, 5,
			    1, 2, 5, //face right
			    1, 5, 6,
			    0, 7, 4, //face left
			    0, 4, 3,
			    5, 4, 7, //face back
			    5, 7, 6,
			    0, 6, 7, //face bottom
			    0, 1, 6
		    };
			
		    //Mesh mesh = new Mesh();
		    //mesh.vertices = vertices;
		    //mesh.triangles = triangles;
		    ////mesh.Optimize ();
		    //mesh.RecalculateNormals ();
      //      return mesh;

            
            return new MeshData() { vertices = vertices, triangles = triangles };
	    }
    }
}