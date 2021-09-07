
namespace LGen.LParse 
{ 
    public class GrowthProfile
    {
        public int Iterations { get; private set; }

        public GrowthProfile(int iterations) { this.Iterations = iterations; }

        public virtual int GetGrowth(int iteration) { return 999; }
    }
}