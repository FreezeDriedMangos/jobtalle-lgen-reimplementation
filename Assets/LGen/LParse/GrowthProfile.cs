
namespace LGen.LParse 
{ 
    public abstract class GrowthProfile
    {
        public int Iterations { get; private set; }

        public GrowthProfile(int iterations) { this.Iterations = iterations; }
        
        public abstract int GetGrowth(int iteration);

        public abstract int GetMaxNumForwardSymbols(int iteration);

        // out-of-the-box profiles
        public class Quadratic : GrowthProfile
        {
            public float initial = 5;
            public float multiplier = 1;

            public int maxFinalForwardSymbols;

            public Quadratic(int iterations) : base(iterations) { }
            public Quadratic(int iterations, float initial, float multiplier, int maxFinalForwardSymbols) : this(iterations)
            {
                this.initial = initial;
                this.multiplier = multiplier;
                this.maxFinalForwardSymbols = maxFinalForwardSymbols;
            }

            public override int GetGrowth(int iteration)
            {
                return (int)(initial + multiplier*iteration*iteration);
            }

            public override int GetMaxNumForwardSymbols(int iteration)
            {
                return maxFinalForwardSymbols;
            }
        }

        public class Unbounded : GrowthProfile
        {
            public Unbounded(int iterations) : base(iterations) { }
            public override int GetGrowth(int iteration)
            {
                return int.MaxValue;
            }

            public override int GetMaxNumForwardSymbols(int iteration)
            {
                return int.MaxValue;
            }
        }
    }
}