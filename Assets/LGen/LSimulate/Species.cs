using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGen.LSimulate { 
    public class Species
    {
        public Agent representative;
        public uint id { get { return representative.speciesID; } }

        public uint parentSpeciesID = 0;
    }
}
