
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
    }

    public class AgentMeshes
    {
        public List<Mesh> stemMeshes = new List<Mesh>();
        public List<Mesh> seedMeshes = new List<Mesh>();
        public List<Mesh> leafMeshes = new List<Mesh>();
    }
}