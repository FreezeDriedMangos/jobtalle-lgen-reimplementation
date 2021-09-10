
using LGen.LEvolve;
using LGen.LParse;
using LGen.LRender;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LGen.LSimulate
{
    public class Agent
    {
        public Vector2Int location;
        public Sentence sentence;
        public LSystem system;
        public AgentRenderData renderData;

        public float viability_sunlightExposure;
        public float viability_stability;
        public float viability_efficiency;
        public bool invalid;
        public float viability { get { return invalid? -1 : viability_sunlightExposure * viability_stability * viability_efficiency; } }
    }

    public class SimulationState
    {
        public Grid grid;
        public List<Agent> agents = new List<Agent>();
    }


    public class Simulation : Singleton<Simulation>
    {
        public bool LOG_LEAF_EXPOSURES;

        public int randomizerSeed;

        public float gridScale;
        public int gridWidth;
        public int gridHeight;

        public int numGrowthIterationsInMaxFertility;
        public int numGrowthIterationsInMinFertility;

        public float quadraticGrowthProfileMultiplier;
        public float quadraticGrowthProfileAddition;

        public Randomizer randomizer;

        public float leafSizePenaltyFactor;
        public float instabilityPenaltyFactor;
        public float ruleCountPenaltyFactor;
        public float seedCountPenaltyFactor;

        public int maxNumSeedsPerAgent;
        public float seedDistributionAlpha;
        public int densityThreshold;

        public string initialAxiom;
        public string[] initialRules;

        public Texture2D fertilityMap;

        public float leafOpacity = 0.8f;
        public float maxExpectedBranchLoad_displayPurposesOnly = 2f;

        public SimulationState state;

        public Simulation() { randomizer = new Randomizer(randomizerSeed); }
        public void Start()
        {
            state = InitializeState(true);
        }


        public SimulationState InitializeState(bool startIteration = false)
        {
            SimulationState state = new SimulationState();
            InitializeGrid(state, fertilityMap);
            InitializeAgents(state, new LSystem(initialAxiom, initialRules));
            if (startIteration) IterateFirstHalf(state);

            return state;
        }

        public void IterateFirstHalf(SimulationState state)
        {
            BuildAgentSentences(state);
            RenderAgents_AndEvaluateLeafExposure(state);
            EvaluateAgentsViability(state);
        }

        public void IterateSecondHalf(SimulationState state)
        {
            SeedNextState(state);
        }

        public void Iterate_SecondHalfFirst(SimulationState state)
        {
            IterateSecondHalf(state);
            CleanState(state);
            IterateFirstHalf(state); // I want to end with a bunch of plants rendered and their data stored, IterateFirstHalf erases their data
        }





        public void InitializeGrid(SimulationState state, Texture2D fertilityMap)
        {
            state.grid = new Grid(gridWidth, gridHeight, fertilityMap);
        }

        public void InitializeAgents(SimulationState state, LSystem templateAgentSystem)
        {
            for(int x = 0; x < this.gridWidth; x++)
            {
                for(int y = 0; y < this.gridHeight; y++)
                {
                    Agent a = new Agent();
                    a.location = new Vector2Int(x, y);
                    a.system = new LSystem(templateAgentSystem);
                    
                    state.grid[x, y].occupant = a;
                    state.agents.Add(a);
                }
            }
        }


        public void BuildAgentSentences(SimulationState state)
        {
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];
                
                float fertility = state.grid[agent.location.x, agent.location.y].fertility;
                int iterations = (int)Mathf.Ceil((numGrowthIterationsInMaxFertility-numGrowthIterationsInMinFertility)*fertility + numGrowthIterationsInMinFertility);
                GrowthProfile growthProfile = new GrowthProfile.Quadratic(iterations, quadraticGrowthProfileMultiplier, quadraticGrowthProfileAddition);        

                agent.sentence = agent.system.Generate(growthProfile, this.randomizer);
            }
        }

        
        public void RenderAgents_AndEvaluateLeafExposure(SimulationState state)
        {
            LRender.Renderer renderer = new LRender.Renderer();
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];
                agent.renderData = renderer.Render(agent.sentence, this.randomizer, i, this.transform, this.leafOpacity, state.grid[agent.location.x, agent.location.y].fertility, this.maxExpectedBranchLoad_displayPurposesOnly);
                agent.renderData.gameObject.transform.position = new Vector3(agent.location.x * this.gridScale, 0, agent.location.y * this.gridScale);
            }

            
            renderer.EvaluateExposure(state.agents.ConvertAll(new System.Converter<Agent, AgentRenderData>((Agent agent) => agent.renderData)));

            if (LOG_LEAF_EXPOSURES)
            { 
                List<float> exposures = state.agents.ConvertAll(new System.Converter<Agent, float>((Agent agent) => agent.renderData.agentData.exposureReport.exposure));
                Debug.Log("[" + string.Join(", ", exposures) + "]");
            }
        }

        public void EvaluateAgentsViability(SimulationState state)
        {
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];
                AgentData agentData = agent.renderData.agentData;
                
                if (agentData.limitsReport.minimum.y < 0)
                {
                    // agent is growing underground
                    agent.invalid = true;
                    continue;
                }

                //
                // sunlight exposure
                //

                float leafEfficiencyFactor = 0;
                int numLeaves = agentData.leafReports.Count;
                for(int l = 0; l < numLeaves; l++)
                {
                    LeafReport r = agentData.leafReports[l];
                    float areaFactor = r.area * this.leafSizePenaltyFactor;
                    leafEfficiencyFactor += (1 - areaFactor*areaFactor) / ((float)numLeaves);
                }

                if (numLeaves == 0) leafEfficiencyFactor = 1;
                agent.viability_sunlightExposure = leafEfficiencyFactor * agentData.exposureReport.exposure / (float)agent.sentence.Tokens.Count;

                //
                // stability
                //
                float lean = Vector3.Distance(agentData.positionReport.centerOfGravity, agentData.positionReport.rootPosition);
                agent.viability_stability = 1f / (1 + instabilityPenaltyFactor * lean * lean);

                //
                // efficiency
                //
                if (agent.system.Rules.Count == 0 || agentData.seedReports.Count == 0) agent.viability_efficiency = 1;
                else agent.viability_efficiency = 1f / (ruleCountPenaltyFactor * (float)agent.system.Rules.Count * seedCountPenaltyFactor * (float)agentData.seedReports.Count);
            }
        }

        public void CleanState(SimulationState state)
        {
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];
                Destroy(agent.renderData.gameObject);
            }
        }

        public void SeedNextState(SimulationState state) 
        {
            List<Seed> seeds = new List<Seed>(); 
            Mutator mutator = new Mutator();
            MutationProfile mutationProfile = new MutationProfile();
            
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];
                AgentData agentData = agent.renderData.agentData;
                float agentRadius = agentData.limitsReport.Radius;
                
                for(int j = 0; j < agentData.seedReports.Count && j < maxNumSeedsPerAgent; j++)
                {
                    Seed seed = new Seed();
                    seed.system = mutator.Mutate(agent.system, mutationProfile, this.randomizer);
                    seed.parentRadius = agentRadius;
                    seed.parentViability = agent.viability;

                    seed.locationRelativeToParentRoot = agentData.seedReports[j].location; // TODO: make sure that seedReport.location really is relative to parent root
                    seed.parentGridLocation = agent.location;
                    
                    seed.absoluteLocation = this.gridScale*(new Vector3(seed.parentGridLocation.x, 0, seed.parentGridLocation.y)) + seed.locationRelativeToParentRoot;
                }
            }

            for(int x = 0; x < this.gridWidth; x++)
            {
                for(int y = 0; y < this.gridHeight; y++)
                {
                    state.grid[x, y].occupant = null;
                }
            }

            // shuffle seeds
            seeds.OrderBy(seed => randomizer.MakeFloat(0, 99));
            // sort seeds by parentViability
            seeds.OrderBy(seed => seed.parentViability);

            // distribute seeds

            // radius of a cone given angle and height
        
            for(int i = 0; i < seeds.Count; i++)
            {
                Seed seed = seeds[i];
                float distributionRadius = Mathf.Tan(seedDistributionAlpha) * seed.absoluteLocation.y; // a cone has a right triangle as its sillhouette
                float r = randomizer.MakeFloat(0, distributionRadius);
                float theta = randomizer.MakeFloat(-Mathf.PI, Mathf.PI);
                
                Vector2 absoluteLocation = new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
                Vector2Int gridLocation = new Vector2Int(Mathf.RoundToInt(absoluteLocation.x / gridScale), Mathf.RoundToInt(absoluteLocation.y / gridScale));
                gridLocation = new Vector2Int(System.Math.Max(System.Math.Min(this.gridWidth-1, gridLocation.x), 0), System.Math.Max(System.Math.Min(this.gridHeight-1, gridLocation.y), 0));

                if (state.grid[gridLocation.x, gridLocation.y].density >= densityThreshold) continue;
                if (state.grid[gridLocation.x, gridLocation.y].occupant != null) continue;

                Agent agent = new Agent();
                agent.location = gridLocation;
                agent.system = seed.system;
                
                state.agents.Add(agent);
                state.grid[gridLocation.x, gridLocation.y].occupant = agent;

                // iterating over gridtiles within circle code from https://stackoverflow.com/a/41136531/9643841
                //for (int iy = - radius  to  radius; iy++)
                //    dx = (int) sqrt(radius * radius - iy * iy)
                //    for (int ix = - dx  to  dx; ix++)
                //        doSomething(CX + ix, CY + iy);
                float radius = seed.parentRadius;
                for (float iy = -radius; iy <  radius; iy++)
                {
                    float dx = Mathf.Sqrt(radius * radius - iy * iy);
                    for (float ix = -dx; ix < dx; ix++)
                    {
                        int gx = Mathf.CeilToInt(ix + gridLocation.x);
                        int gy = Mathf.CeilToInt(iy + gridLocation.y);
                        state.grid[gx, gy].density++;
                    }
                }
            }
        }

        class Seed
        {
            public LSystem system;
            public float parentRadius;
            public float parentViability;
            public Vector3 locationRelativeToParentRoot;
            public Vector2Int parentGridLocation;

            public Vector3 absoluteLocation;
        }
    }
}