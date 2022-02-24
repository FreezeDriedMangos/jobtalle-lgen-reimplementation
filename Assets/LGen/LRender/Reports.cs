

using UnityEngine;

namespace LGen.LRender
{
    public class SeedReport
    {
        public Vector3 location;

        public SeedReport(Vector3 v) { this.location = v; }
    }

    public class LimitsReport
    {
        public Vector3 minimum;
        public Vector3 maximum;

        public float Radius { get { return Mathf.Sqrt((maximum.x - minimum.x) * (maximum.y - minimum.y) / Mathf.PI); } }

        public void Add(Vector3 v)
        {
            minimum.x = Mathf.Min(minimum.x, v.x);
            minimum.y = Mathf.Min(minimum.y, v.y);
            minimum.z = Mathf.Min(minimum.z, v.z);
            
            maximum.x = Mathf.Max(maximum.x, v.x);
            maximum.y = Mathf.Max(maximum.y, v.y);
            maximum.z = Mathf.Max(maximum.z, v.z);
        }
    }

    public class LeafReport
    {
        public float area;
    }

    public class ExposureReport
    {
        public float exposure;
    }

    public class PositionReport
    {
        public Vector3 rootPosition;
        public Vector3 centerOfGravity;
    }

    public class BranchReport
    {
        public int branchLoad;
    }

    public class StructureReport
    {
        public int maxStructureDepth;
    }
}