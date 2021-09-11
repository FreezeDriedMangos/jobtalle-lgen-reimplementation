
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
        public int generation;
    }


    public class Simulation : Singleton<Simulation>
    {
        public bool LOG_LEAF_EXPOSURES;

        public int randomizerSeed;

        public float gridScale;
        public int gridWidth;
        public int gridHeight;
        public int placeInitialSeedEveryNTiles;

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

        public float initialMutability;
        public float mutabilityFalloff;

        public float stemRadiusFactor = 0.02f;
        public float seedSize = 0.2f;

        public SimulationState state;

        //
        // Top level functions
        //

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
            state.generation++;
            SeedNextState(state);
        }

        public void Iterate_SecondHalfFirst(SimulationState state)
        {
            IterateSecondHalf(state);
            IterateFirstHalf(state); // I want to end with a bunch of plants rendered and their data stored, IterateFirstHalf erases their data
        }


        //
        // Misc functions
        //

        public void AddColliders(SimulationState state)
        {
            foreach(Agent a in state.agents)
            {
                //BoxCollider c = a.renderData.gameObject.AddComponent<BoxCollider>();
                //Vector3 size = a.renderData.agentData.limitsReport.maximum - a.renderData.agentData.limitsReport.minimum;
                //c.size = size == Vector3.zero ? new Vector3(0.25f, 0.25f, 0.25f) : size;
                //c.center = size == Vector3.zero ? new Vector3(0.125f, 0, 0) : a.renderData.agentData.limitsReport.minimum + 0.5f * c.size;                
                foreach(MeshFilter f in a.renderData.gameObject.GetComponentsInChildren<MeshFilter>())
                {
                    MeshCollider c = f.gameObject.AddComponent<MeshCollider>();
                    c.sharedMesh = f.sharedMesh;
                }
            }
        }

        public string PrintAgentForGameObject(GameObject o)
        {
            foreach(Agent a in state.agents)
            {
                if (a.renderData.gameObject == o)
                {
                    string s = "Sentence: "+a.sentence+"\n\nSystem:\n"+a.system+
                            "\n\nInvalid?" + a.invalid + "\nViability: "+a.viability+"\nViability Sunlight: "+a.viability_sunlightExposure+"\nViability Stability: "+a.viability_stability+"\nViability Efficiency: "+a.viability_efficiency+ 
                            "\n\nLimits Minimum: "+a.renderData.agentData.limitsReport.minimum+"\nLimitsMaximum: "+a.renderData.agentData.limitsReport.maximum+"\nRadius: "+a.renderData.agentData.limitsReport.Radius+
                            "\n\nSystem E Set: " + (new GeneratedSymbols(a.system)).ToString() +
                            "\n\n"+a.renderData.agentData.exposureReport.exposure;
                            ;
                    return s;
                }
            }

            return "No matching agent found.";
        }

        public void ShowGridDensity(SimulationState state, GameObject parent, UnityEngine.UI.Text textPrefab)
        {
            
            for(int x = 0; x < this.gridWidth; x++)
            {
                for(int y = 0; y < this.gridHeight; y++)
                {
                    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    g.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    g.transform.position = new Vector3(x*this.gridScale, 0, y*this.gridScale);
                    g.transform.parent = parent.transform;

                    UnityEngine.UI.Text t = GameObject.Instantiate(textPrefab);
                    t.transform.position = g.transform.position + new Vector3(0, 0.15f, 0);
                    t.text = ""+state.grid[x, y].density;
                    t.gameObject.transform.parent = parent.transform;
                }
            }
        }


        //
        // Core functions
        //

        public void InitializeGrid(SimulationState state, Texture2D fertilityMap)
        {
            state.grid = new Grid(gridWidth, gridHeight, fertilityMap);
        }

        public void InitializeAgents(SimulationState state, LSystem templateAgentSystem)
        {
            for(int x = 0; x < this.gridWidth; x += placeInitialSeedEveryNTiles)
            {
                for(int y = 0; y < this.gridHeight; y += placeInitialSeedEveryNTiles)
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
                agent.renderData = renderer.Render(agent.sentence, this.randomizer, i, this.transform, this.leafOpacity, state.grid[agent.location.x, agent.location.y].fertility, this.maxExpectedBranchLoad_displayPurposesOnly, this.stemRadiusFactor, this.seedSize);
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
                else 
                {
                    float ruleEfficiency = ruleCountPenaltyFactor * (float)agent.system.Rules.Count;
                    ruleEfficiency = ruleEfficiency == 0 ? 1 : ruleEfficiency;
                    float seedEfficiency = seedCountPenaltyFactor * (float)agentData.seedReports.Count;
                    seedEfficiency = seedEfficiency == 0 ? 1 : seedEfficiency;

                    agent.viability_efficiency = 1f / (ruleEfficiency*seedEfficiency);
                }   
            }
        }

        public void SeedNextState(SimulationState state) 
        {
            List<Seed> seeds = new List<Seed>(); 
            Mutator mutator = new Mutator();
            MutationProfile mutationProfile = new MutationProfile(1 + this.initialMutability / (1+state.generation*this.mutabilityFalloff));
            
            //
            // generate seeds
            //
            
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];
                if (agent.viability < 0) continue;

                AgentData agentData = agent.renderData.agentData;
                float agentRadius = agentData.limitsReport.Radius;
                
                for(int j = 0; j < agentData.seedReports.Count && j < maxNumSeedsPerAgent; j++)
                {
                    Seed seed = new Seed();
                    seed.system = mutator.Mutate(agent.system, mutationProfile, this.randomizer);
                    seed.parentRadius = Mathf.Max(agentRadius, 0.51f);
                    seed.parentViability = agent.viability;

                    seed.locationRelativeToParentRoot = agentData.seedReports[j].location; // TODO: make sure that seedReport.location really is relative to parent root
                    seed.parentGridLocation = agent.location;
                    
                    seed.absoluteLocation = this.gridScale*(new Vector3(seed.parentGridLocation.x, 0, seed.parentGridLocation.y)) + seed.locationRelativeToParentRoot;
                    seeds.Add(seed);
                }
            }
            
            //
            // Remove old agents
            //
                        
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];
                Destroy(agent.renderData.gameObject);
            }

            state.agents.Clear();

            for(int x = 0; x < this.gridWidth; x++)
            {
                for(int y = 0; y < this.gridHeight; y++)
                {
                    state.grid[x, y].density = 0;
                    state.grid[x, y].occupant = null;
                }
            }

            //
            // place new agents
            //

            // shuffle seeds
            seeds = seeds.OrderBy(seed => randomizer.MakeFloat(0, 99)).ToList<Seed>();
            // sort seeds by parentViability
            seeds = seeds.OrderBy(seed => -seed.parentViability).ToList<Seed>(); // orderby sorts things in ascending order, I want descending, so I make parent viabiilty negative
            
            // distribute seeds
            for(int i = 0; i < seeds.Count; i++)
            {
                Seed seed = seeds[i];
                float distributionRadius = Mathf.Tan(seedDistributionAlpha) * seed.absoluteLocation.y; // a cone has a right triangle as its sillhouette
                float r = randomizer.MakeFloat(0, distributionRadius);
                float theta = randomizer.MakeFloat(-Mathf.PI, Mathf.PI);
                
                Vector2 absoluteLocation = new Vector2(r * Mathf.Cos(theta) + seed.parentGridLocation.x*gridScale, r * Mathf.Sin(theta) + seed.parentGridLocation.y*gridScale);
                Vector2Int gridLocation = new Vector2Int(Mathf.RoundToInt(absoluteLocation.x / gridScale), Mathf.RoundToInt(absoluteLocation.y / gridScale));
                gridLocation = new Vector2Int(System.Math.Max(System.Math.Min(this.gridWidth-1, gridLocation.x), 0), System.Math.Max(System.Math.Min(this.gridHeight-1, gridLocation.y), 0));

                if (state.grid[gridLocation.x, gridLocation.y].density >= this.densityThreshold) continue;
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
                int radius = Mathf.CeilToInt(seed.parentRadius/this.gridScale); // radius in grid coords
                for (int iy = -radius; iy < radius; iy++)
                {
                    int dx = Mathf.CeilToInt(Mathf.Sqrt(radius * radius - iy * iy));
                    for (int ix = -dx; ix < dx; ix++)
                    {
                        int gx = ix + gridLocation.x;
                        int gy = iy + gridLocation.y;
                        if (gx < 0 || gy < 0 || gx >= gridWidth || gy >= gridHeight) continue;
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