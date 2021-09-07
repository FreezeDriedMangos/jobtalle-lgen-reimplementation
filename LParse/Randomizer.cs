

using System;

namespace LGen.LParse 
{ 
    public class Randomizer 
    {
        private Random random;
        
        public Randomizer() { this.random = new Random(); }
        public Randomizer(string json) { throw new NotImplementedException(); }
        public Randomizer(Int32 seed) { this.random = new Random(seed); }
        
        public int MakeInt_Inclusive(int min, int max) { return this.random.Next(min, max+1); }
        public int MakeInt_Exclusive(int min, int max) { return this.random.Next(min, max); }
        public float MakeFloat(float min, float max) { return (float)(this.random.NextDouble() * (max-min)) + min; }

        public static Random FromJSON(string json) { throw new NotImplementedException(); }
        public static string ToJSON(Randomizer r) { throw new NotImplementedException(); }
    }
}
