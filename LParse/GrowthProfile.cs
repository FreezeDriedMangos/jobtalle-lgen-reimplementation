
namespace LGen.LParse 
{ 
    public abstract class GrowthProfile
    {
        public int Iterations { get; private set; }

        public GrowthProfile(int iterations) { this.Iterations = iterations; }

        public abstract int GetGrowth(int iteration);

        // out-of-the-box profiles
        public class Quadratic : GrowthProfile
        {
            public int initial;
            public int multiplier;

            public Quadratic(int iterations) : base(iterations) { }
            public Quadratic(int iterations, int initial, int multiplier) : this(iterations)
            {
                this.initial = initial;
                this.multiplier = multiplier;
            }

            public override int GetGrowth(int iteration)
            {
                return initial + multiplier*iteration*iteration;
            }
        }

        public class Unbounded : GrowthProfile
        {
            public Unbounded(int iterations) : base(iterations) { }
            public override int GetGrowth(int iteration)
            {
                return int.MaxValue;
            }
        }
    }
}