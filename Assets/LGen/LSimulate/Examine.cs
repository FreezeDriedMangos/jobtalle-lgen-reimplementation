using LGen.LEvolve;
using LGen.LParse;
using LGen.LRender;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using UnityEngine.UI;

namespace LGen.LSimulate 
{
    // Examine a single genome
    public class Examine : Simulation
    {
        public string genomeAxiom;
        public string[] genomeRules;

        public string concatenatedFullGenome;
        
        public int gridX = 0;
        public int gridY = 0;

        private Agent agent;
        public Text debugText;

        public void Update()
        {
            if (debugText == null) return;

            string s = this.PrintAgentForGameObject(this.agent.renderData.gameObject);
            debugText.text = s;
        }

        public override void InitializeAgents(SimulationState state, LSystem templateAgentSystem)
        {
            Agent a = new Agent();
            a.location = new Vector2Int(gridX, gridY);
            a.system = new LSystem(templateAgentSystem);
                    
            state.grid[gridX, gridY].occupant = a;
            state.agents.Add(a);
            this.agent = a;

            Speciate(state, a, 0);
        }
    }
}