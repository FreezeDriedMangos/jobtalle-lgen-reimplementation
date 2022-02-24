
using System.Collections.Generic;
using UnityEngine;

namespace LGen.LRender
{
    public class AgentData
    {
        public AgentMeshes meshes = new AgentMeshes();

        public List<SeedReport> seedReports = new List<SeedReport>(); 
        public List<LeafReport> leafReports = new List<LeafReport>();
        public LimitsReport limitsReport = new LimitsReport();
        public ExposureReport exposureReport = new ExposureReport();
        public PositionReport positionReport = new PositionReport();
        public List<BranchReport> branchReports = new List<BranchReport>();
        public StructureReport structureReport = new StructureReport();
    }

    public class AgentMeshes
    {
        public List<Mesh> stemMeshes = new List<Mesh>();
        public List<Mesh> seedMeshes = new List<Mesh>();
        public List<Mesh> leafMeshes = new List<Mesh>();

        public List<MeshData> uncompiledStemMeshes = new List<MeshData>();
        public List<MeshData> uncompiledSeedMeshes = new List<MeshData>();
        public List<MeshData> uncompiledLeafMeshes = new List<MeshData>();
    }

    public class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;

        // optional stuff - branches
        public float branch_topRadius;
        public float branch_bottomRadius;
        public Quaternion branch_rotation;
        public Vector3 branch_topLocation;
        public float branch_length;

        // optional stuff - seeds
        public Vector3 seed_location;
        public float seed_size;
        
        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
        
            mesh.vertices = vertices;
            mesh.triangles = triangles;
           if (mesh != null) mesh.uv = uvs;
           mesh.RecalculateNormals();

            return mesh;
        }
    }
}