
using LGen.LEvolve;
using LGen.LParse;
using LGen.LRender;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

using QuadTreeLib;
using System.Drawing;

namespace LGen.LSimulate
{
    public class Agent
    {
        private static uint ID_COUNTER = 0;
        public uint id = ID_COUNTER++;
        private static uint SPECIES_COUNTER = 1;
        public uint speciesID = 0;


        public Vector2Int location;
        public Sentence sentence;
        public LSystem system;
        public AgentRenderData renderData;

        public float viability_sunlightExposure;
        public float viability_stability;
        public float viability_seeds;
        public float viability_rules;
        public float viability_leaves;
        public float viability_depth;


        public bool invalid;

        public float viability { get { return invalid? -1 : viability_sunlightExposure * viability_stability * viability_seeds * viability_rules * viability_leaves * viability_depth; } }
    }

    public class SimulationState
    {
        public string simulationID = System.Guid.NewGuid().ToString();
        public Grid grid;
        public List<Agent> agents = new List<Agent>();
        public int generation;

        // when a new species is created, record it to disk under {simulationID}/species_{speciesID}.JSON
        // remove species with 0 members
        public List<Species> species = new List<Species>();
    }


    public class Simulation : Singleton<Simulation>
    {
        public bool LOG_LEAF_EXPOSURES;

        public int randomizerSeed;

        public float gridScale;
        public int gridWidth;
        public int gridHeight;
        public int placeInitialSeedEveryNTiles;

        public Randomizer randomizer;

        //public float leafSizePenaltyFactor;
        //public float instabilityPenaltyFactor;
        //public float ruleCountPenaltyFactor;
        //public float seedCountPenaltyFactor;

        public int maxNumSeedsPerAgent;
        public float seedDistributionAlpha;
        public int densityThreshold;
        public int sentenceLengthLimit;
        public int maxNumForwardSymbolsInFinishedSentence;

        public string initialAxiom;
        public string[] initialRules;

        public Texture2D fertilityMap;

        public float leafOpacity = 0.8f;
        public float maxExpectedBranchLoad_displayPurposesOnly = 2f;

        public float initialMutability;
        public float mutabilityFalloff;

        public float stemRadiusFactor = 0.02f;
        public float seedSize = 0.2f;

        public float GROWTH_PROFILE_RESOLUTION = 10;

        public SimulationState state;

        public float DEFAULT_SPREAD = 0.05f;

        public const float TURTLE_ANGLE_DELTA = (float)(Mathf.PI/9f);
        public float branchLength = 0.5f;
        public float seedOffset = 0;

        public bool limitAgentCount = false;
        public int maxNumAgents = 50;

        public bool writeSpeciesToDisc;
        public float maxSpeciesDelta;
        public float speciation_c1;
        public float speciation_c2;
        public float speciation_c3;

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
            Stopwatch sw = new Stopwatch();
            sw.Start();
                BuildAgentSentences(state);
            sw.Stop();
            TimeKeeper.Instance.BuildSentences = sw.Elapsed.ToString();

            
            RenderAgents_AndEvaluateLeafExposure(state);

            sw = new Stopwatch();
            sw.Start();
                EvaluateAgentsViability(state);
            sw.Stop();
            TimeKeeper.Instance.EvaluateViability = sw.Elapsed.ToString();
        }

        public void IterateSecondHalf(SimulationState state)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            state.generation++;
            SeedNextState(state);

            sw.Stop();
            TimeKeeper.Instance.SeedNextState = sw.Elapsed.ToString();
        }

        public void Iterate_SecondHalfFirst(SimulationState state)
        {
            IterateSecondHalf(state);
            IterateFirstHalf(state); // I want to end with a bunch of plants rendered and their data stored, IterateFirstHalf erases their data
        }


        //
        // Misc functions
        //

        public void JSONtoFile(SimulationState state, System.IO.StreamWriter writer)
        {
            string s = "";
            
            s += "{\n";
            s += $"\t\"uuid\": \"{state.simulationID}\",\n";

            s += "    \"generation\":" + state.generation + ",\n";
            
            s += "    \"agents\":[";
            writer.Write(s);
            s = "";    

            for(int i = 0; i < state.agents.Count; i++) 
            {
                Agent a = state.agents[i];

                string ruleIds = "[";
                foreach(Rule r in a.system.Rules) ruleIds += r.id + ", ";

                if (a.system.Rules.Count > 0) ruleIds = ruleIds.Substring(0, ruleIds.Length-2)+"]";
                else ruleIds += "]";

                s += "\n";
                s += "        {\n";
                s += $"            \"location\": [{a.location.x}, {a.location.y}],\n"; // this and below used for calculation / simulation
                s += $"            \"system\": \"{a.system.ToString().Replace("\\", "\\\\").Replace("\n", "\\n")}\"\n";
                s += $"            \"id\": {a.id},\n"; // this and below not used for calculation / simulation
                s += $"            \"species\": {a.speciesID},\n";
                s += $"            \"rule_ids\": {ruleIds}\n";
                s += "        }";
                
                if (i < state.agents.Count-1) s += ",";

                writer.Write(s);
                s = "";
            }
            
            s += "\n    ],\n";
            s += "    \"randomizerState\": " + randomizer.GetStateAsJSON() + "\n";
            s += "}\n";
        
            writer.Write(s);
            s = "";

            writer.Close();
        }

        public static void WriteSpeciesToJSON(SimulationState state, Species sp)
        {
            if(!Simulation.Instance.writeSpeciesToDisc) return;

            string directory = $"Assets/JSON/{state.simulationID}/species";
            System.IO.Directory.CreateDirectory(directory);
            System.IO.StreamWriter writer = new System.IO.StreamWriter($"{directory}/species_{sp.id}.json", true);
            string s = "";
            
            s += "{\n";
            s += $"    \"simulation_uuid\": \"{state.simulationID}\",\n";
            s +=  "    \"generation\":" + state.generation + ",\n";
            s += $"    \"speciesId\":{sp.id},\n";
            s += $"    \"representative\": \"{sp.representative.system.ToString().Replace("\\", "\\\\").Replace("\n", "\\n")}\",\n";
            s += $"    \"parent_species_id\":{sp.parentSpeciesID}\n";
            s +=  "}\n";
            writer.Write(s);
            
            writer.Close();
        }

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
                            "\n\nInvalid?" + a.invalid + "\nViability: "+a.viability+"\nViability Sunlight: "+a.viability_sunlightExposure+"\nViability Stability: "+a.viability_stability+"\nViability Seeds: "+a.viability_seeds+"\nViability Rules: "+a.viability_rules+"\nViability Leaves: "+a.viability_leaves+"\nViability Structure Depth"+a.viability_depth+ 
                            "\n\nLeaf Areas" + string.Join(", ", a.renderData.agentData.leafReports.Select(leafReport => leafReport.area).ToArray()) +"\n"+
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

        public virtual void InitializeAgents(SimulationState state, LSystem templateAgentSystem)
        {
            for(int x = 0; x < this.gridWidth; x += placeInitialSeedEveryNTiles)
            {
                for(int y = 0; y < this.gridHeight; y += placeInitialSeedEveryNTiles)
                {
                    if (this.limitAgentCount && state.agents.Count >= this.maxNumAgents) break;

                    Agent a = new Agent();
                    a.location = new Vector2Int(x, y);
                    a.system = new LSystem(templateAgentSystem);
                    
                    state.grid[x, y].occupant = a;
                    state.agents.Add(a);

                    Speciate(state, a, 0);
                }
            }
        }


        public void BuildAgentSentences(SimulationState state)
        {
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];
                
                float fertility = 1 - state.grid[agent.location.x, agent.location.y].fertility;
                
                int x = Mathf.FloorToInt(fertility * GROWTH_PROFILE_RESOLUTION);
                int iterations = 1 + Mathf.CeilToInt(4*(1-x/GROWTH_PROFILE_RESOLUTION));
                int initial = 24; // // Problem 2: Plants were growing enormously long main trunks - solution: limit growth further
                float multiplier = (GROWTH_PROFILE_RESOLUTION - x)*10f;

                // Problem 4: Plants were still growing enormously long - solution: limit number of forward symbols
                GrowthProfile growthProfile = new GrowthProfile.Quadratic(iterations, initial, multiplier, maxNumForwardSymbolsInFinishedSentence);        
                agent.sentence = agent.system.Generate(growthProfile, this.randomizer);
            }
        }

        
        public void RenderAgents_AndEvaluateLeafExposure(SimulationState state)
        {
                Stopwatch sw = new Stopwatch();
                sw.Start();

            LRender.Renderer renderer = new LRender.Renderer();
            //for(int i = 0; i < state.agents.Count; i++)
            //{
            //    Agent agent = state.agents[i];
            //    agent.renderData = renderer.Render(agent.sentence, this.randomizer, i, this.transform, this.leafOpacity, state.grid[agent.location.x, agent.location.y].fertility, this.maxExpectedBranchLoad_displayPurposesOnly, this.stemRadiusFactor, this.seedSize, this.branchLength, TURTLE_ANGLE_DELTA, this.seedOffset);
            //    agent.renderData.gameObject.transform.position = new Vector3(agent.location.x * this.gridScale, 0, agent.location.y * this.gridScale);
            //}

            List<Sentence> sentences = state.agents.ConvertAll(new System.Converter<Agent, Sentence>((agent) => agent.sentence));
            List<float> fertilities = state.agents.ConvertAll(new System.Converter<Agent, float>((agent) => state.grid[agent.location.x, agent.location.y].fertility ));
            List<AgentRenderData> agentRenderData = renderer.Render(sentences, this.randomizer, this.transform, this.leafOpacity, fertilities, this.maxExpectedBranchLoad_displayPurposesOnly, this.stemRadiusFactor, this.seedSize, this.branchLength, TURTLE_ANGLE_DELTA, this.seedOffset);
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];
                agent.renderData = agentRenderData[i];
                agent.renderData.gameObject.transform.position = new Vector3(agent.location.x * this.gridScale, 0, agent.location.y * this.gridScale);
            }




                sw.Stop();
                TimeKeeper.Instance.RenderAgents = sw.Elapsed.ToString();

                sw = new Stopwatch();
                sw.Start();

            renderer.EvaluateExposure(state.agents.ConvertAll(new System.Converter<Agent, AgentRenderData>((Agent agent) => agent.renderData)));

                sw.Stop();
                TimeKeeper.Instance.EvaluateLeafExposure = sw.Elapsed.ToString();

            if (LOG_LEAF_EXPOSURES)
            { 
                List<float> exposures = state.agents.ConvertAll(new System.Converter<Agent, float>((Agent agent) => agent.renderData.agentData.exposureReport.exposure));
                UnityEngine.Debug.Log("[" + string.Join(", ", exposures) + "]");
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

                float exposureReport = agentData.exposureReport.exposure*1000;
                float divisor = agent.sentence.Tokens.Count * agent.sentence.Tokens.Count; // Problem 1: dominant strategy was to grow leaves that literally blanketed the whole ground - solution: square the divisor
                agent.viability_sunlightExposure = exposureReport * exposureReport / divisor;

                //
                // seeds
                //

                agent.viability_seeds = 1f / (agentData.seedReports.Count * 0.05f + 1);
        
                //
                // Rules
                //

                agent.viability_rules = 1f / (agent.system.Rules.Count * 0.05f + 1);        

                //
                // stability
                //

                float lean = Vector3.Distance(agentData.positionReport.centerOfGravity, agentData.positionReport.rootPosition) + 1;
                agent.viability_stability = 1f / (lean * lean + 1);

                //
                // leaves
                //
                
                int numLeaves = agentData.leafReports.Count;
                if (numLeaves == 0) agent.viability_leaves = 0;
                else { 
                    float leafEfficiencyFactor = 0;
                    for(int l = 0; l < numLeaves; l++)
                    {
                        LeafReport r = agentData.leafReports[l];
                        float areaFactor = r.area * 8f      * 1000f;
                        leafEfficiencyFactor += 1 - areaFactor*areaFactor;
                    }
                    leafEfficiencyFactor /= (float)numLeaves;
                
                    agent.viability_leaves = Mathf.Sign(leafEfficiencyFactor) * leafEfficiencyFactor*leafEfficiencyFactor; // Problem 3: plants never evolved complex leaves, only non-branching leaves. Solution: square the leaf factor (and preserve sign)
                }

                // 
                // depth 
                //

                agent.viability_depth = (30-agentData.structureReport.maxStructureDepth) * 10f; // Problem 4: the best strategy was still to evolve to be ridiculously long and spam leaves. Solution: penalize long, non-branching structures
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

        
            Stopwatch sw = new Stopwatch();
            sw.Start();
            
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

                    seed.parentID = agent.id;
                    seed.parentSpecies = agent.speciesID;
                    
                    seed.absoluteLocation = this.gridScale*(new Vector3(seed.parentGridLocation.x, 0, seed.parentGridLocation.y)) + seed.locationRelativeToParentRoot;
                    seeds.Add(seed);
                }
            }

            

            sw.Stop();
            TimeKeeper.Instance.SeedNextState_GenerateSeeds = sw.Elapsed.ToString();
            
            //
            // Remove old agents
            //

            sw = new Stopwatch();
            sw.Start();
                        
            for(int i = 0; i < state.agents.Count; i++)
            {
                Agent agent = state.agents[i];

                // https://stackoverflow.com/a/48976123/9643841
                List<GameObject> children = new List<GameObject>();
                foreach(Transform child in agent.renderData.gameObject.transform) children.Add(child.gameObject);
                foreach(GameObject child in children)
                {
                    LGen.LRender.Renderer.ReturnObjectToPool(child.gameObject);
                }
                LGen.LRender.Renderer.ReturnObjectToPool(agent.renderData.gameObject);
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

            

            sw.Stop();
            TimeKeeper.Instance.SeedNextState_RemoveOldAgents = sw.Elapsed.ToString();

            //
            // place new agents
            //
            
            sw = new Stopwatch();
            sw.Start();

            // shuffle seeds
            seeds = seeds.OrderBy(seed => randomizer.MakeFloat(0, 99)).ToList<Seed>();
            // sort seeds by parentViability
            seeds = seeds.OrderBy(seed => -seed.parentViability).ToList<Seed>(); // orderby sorts things in ascending order, I want descending, so I make parent viabiilty negative
            
            // TODO: use QuadTree
            // TODO: class QuadTreeSeedItem { Seed seed; float radius; RectangleF Rectangle; int gridx; int gridy }
            //float averageNumGridTilesIterated = 0;

            // distribute seeds
            // QuadTree<QuadTreeSeedItem> quadTree = new QuadTree<QuadTreeSeedItem>(new RectangleF(0, 0, gridWidth, gridHeight));
            for(int i = 0; i < seeds.Count; i++)
            {
                if (this.limitAgentCount && i >= this.maxNumAgents) break;

                //
                // generate seed
                //

                Seed seed = seeds[i];
                //float distributionRadius = Mathf.Tan(seedDistributionAlpha) * seed.absoluteLocation.y; // a cone has a right triangle as its sillhouette
                float r = Mathf.Sqrt(randomizer.MakeFloat(0, 1)) * (seed.locationRelativeToParentRoot.y) + DEFAULT_SPREAD; //randomizer.MakeFloat(0, distributionRadius);
                float theta = randomizer.MakeFloat(-Mathf.PI, Mathf.PI);
                
                Vector2 absoluteLocation = new Vector2(r * Mathf.Cos(theta) + seed.parentGridLocation.x*gridScale, r * Mathf.Sin(theta) + seed.parentGridLocation.y*gridScale);
                Vector2Int gridLocation = new Vector2Int(Mathf.RoundToInt(absoluteLocation.x / gridScale), Mathf.RoundToInt(absoluteLocation.y / gridScale));
                gridLocation = new Vector2Int(System.Math.Max(System.Math.Min(this.gridWidth-1, gridLocation.x), 0), System.Math.Max(System.Math.Min(this.gridHeight-1, gridLocation.y), 0));
                
                //
                // check if seed has valid location
                //
                if (state.grid[gridLocation.x, gridLocation.y].density >= this.densityThreshold) continue;
        
                if (state.grid[gridLocation.x, gridLocation.y].occupant != null) continue;

                // // QuadTree alternate to the density grid
                //
                //int gridspaceRadius = Mathf.CeilToInt(seed.parentRadius/this.gridScale); // radius in grid coords
                //gridspaceRadius += gridspaceRadius %2 == 0? 0 : 1; // make it even for convinence
                //RectangleF queryRect = new RectangleF(gridLocation.x - gridspaceRadius/2, gridLocation.y - gridspaceRadius/2, gridspaceRadius, gridspaceRadius);
                //List<QuadTreeSeedItem> seedItems = quadTree.Query(queryRect);
                //
                //int density = 0;
                //foreach (QuadTreeSeedItem item in seedItems)
                //{
                //    float dx = item.gridLocation.x - gridLocation.x;
                //    float dy = item.gridLocation.y - gridLocation.y;
                //    float distanceSquared = dx*dx + dy*dy;  
                //    
                //    float combinedRadius = item.gridspaceRadius + gridspaceRadius;
                //    float combinedRadiusSquared = combinedRadius*combinedRadius;
                //
                //    if (distanceSquared <= combinedRadiusSquared) density++;
                //    if (density >= this.densityThreshold) break;
                //}
                //
                //if (density >= this.densityThreshold) continue;
                //
                ////
                //// Add seed to quadtree
                ////
                //
                //QuadTreeSeedItem thisItem = new QuadTreeSeedItem();
                //thisItem.rectangle = queryRect;
                //thisItem.seed = seed;
                //thisItem.gridspaceRadius = gridspaceRadius;
                //thisItem.gridLocation = gridLocation;
                //quadTree.Insert(thisItem);


                //
                // create agent from seed (aka germinate seed)
                //

                Agent agent = new Agent();
                agent.location = gridLocation;
                agent.system = seed.system;
                
                state.agents.Add(agent);
                state.grid[gridLocation.x, gridLocation.y].occupant = agent;

                //Speciate(state, agent, seed.parentSpecies); // TODO: This is EXTROARDINARILY slow

                // iterating over gridtiles within circle code from https://stackoverflow.com/a/41136531/9643841
                //for (int iy = - radius  to  radius; iy++)
                //    dx = (int) sqrt(radius * radius - iy * iy)
                //    for (int ix = - dx  to  dx; ix++)
                //        doSomething(CX + ix, CY + iy);
                //float iterCount = 0;
                int radius = Mathf.CeilToInt(seed.parentRadius/this.gridScale); // radius in grid coords
                for (int iy = -radius; iy < radius; iy++)
                {
                    int dx = Mathf.CeilToInt(Mathf.Sqrt(radius * radius - iy * iy));
                    for (int ix = -dx; ix < dx; ix++)
                    {
                        //iterCount++;

                        int gx = ix + gridLocation.x;
                        int gy = iy + gridLocation.y;
                        if (gx < 0 || gy < 0 || gx >= gridWidth || gy >= gridHeight) continue;
                        state.grid[gx, gy].density++;
                    }
                }

                //averageNumGridTilesIterated += iterCount/((float)seeds.Count);
            }
            
            //UnityEngine.Debug.Log("Average num grid tiles iterated per seed: " + averageNumGridTilesIterated);
            //UnityEngine.Debug.Log("Num seeds: " + seeds.Count);
            

            sw.Stop();
            TimeKeeper.Instance.SeedNextState_DistributeSeeds = sw.Elapsed.ToString();

            sw = new Stopwatch();
            sw.Start();

            // remove all empty species
            Dictionary<uint, bool> speciesHasMembers = new Dictionary<uint, bool>();
            foreach(Agent a in state.agents) speciesHasMembers[a.speciesID] = true;
            
            List<Species> speciesToRemove = new List<Species>();
            foreach(Species s in state.species) if (!speciesHasMembers.ContainsKey(s.id) || !speciesHasMembers[s.id]) speciesToRemove.Add(s);
            foreach(Species s in speciesToRemove) state.species.Remove(s);

            

            sw.Stop();
            TimeKeeper.Instance.SeedNextState_ResetSpecies = sw.Elapsed.ToString();
        }

        public void Speciate(SimulationState state, Agent a, uint parentSpecies)
        {
            foreach(Species s in state.species)
            {
                if (Speciation.Delta(s.representative, a, this.speciation_c1, this.speciation_c2, this.speciation_c3) <= this.maxSpeciesDelta)
                {
                    a.speciesID = s.id;
                    return;
                }
            }   

            // create new species with a as the representative, and write it to file
            Species sp = new Species();
            sp.representative = a;
            sp.parentSpeciesID = parentSpecies;
            a.speciesID = Species.ID_COUNTER++;
            
            state.species.Add(sp);
            WriteSpeciesToJSON(state, sp);
        }

        class Seed
        {
            public LSystem system;
            public float parentRadius;
            public float parentViability;
            public Vector3 locationRelativeToParentRoot;
            public Vector2Int parentGridLocation;

            public Vector3 absoluteLocation;

            public uint parentID;
            public uint parentSpecies;
        }

        class QuadTreeSeedItem : IHasRect { 
            public Seed seed; 
            public float gridspaceRadius;
            public RectangleF rectangle; 
            public Vector2Int gridLocation;

            RectangleF IHasRect.Rectangle => rectangle;
        }
    }
}