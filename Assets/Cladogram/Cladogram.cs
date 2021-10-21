using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Cladogram : MonoBehaviour
{
    public static System.Random RANDOM = new System.Random();
    public static float NODE_WIDTH = 1;
    public static float NODE_HEIGHT = 1;
    public static float BUFFER_FACTOR = 1.2f;
    public static float FORCE_DAMPENING = 0.3f;

    public string folderPath;
    public bool strict;
    public bool strongChildPull;

    class SpeciesRecord
    {
        public string simulation_uuid;
        public int generation;
        public int speciesId;
        public int parent_species_id;
        public string representative;
    }

    class Node
    {
        public float x;
        public float y;
        public float height = NODE_HEIGHT;
        public SpeciesRecord species;
        public Node parent;
        public List<Node> children = new List<Node>();

        public int totalNumDescendants;
    }

    int CountDescendants(Node n)
    {
        n.totalNumDescendants = 1;
        foreach(Node child in n.children) n.totalNumDescendants += CountDescendants(n);
        return n.totalNumDescendants;
    }

    void PlaceNodes(Node parent)
    {
        float x = parent.x;
        
        List<Node> leftSide = new List<Node>();
        List<Node> rightSide = new List<Node>();
        
        foreach(Node n in parent.children)
        {
            if (!strict && RANDOM.NextDouble() < 0.5f) leftSide.Add(n);
            else                                       rightSide.Add(n);
        }

        foreach(Node n in leftSide)
        {
            n.x = x;
            
            float dx = NODE_WIDTH*BUFFER_FACTOR * n.totalNumDescendants;
            x += dx;
            parent.x += dx;

            PlaceNodes(n);
        }

        x += NODE_WIDTH*BUFFER_FACTOR;

        foreach(Node n in rightSide)
        {
            n.x = x;
            x += NODE_WIDTH*BUFFER_FACTOR * n.totalNumDescendants;
            PlaceNodes(n);
        }
    }

    bool CouldCollide(Node node, Node nodej)
    {
        if (node == nodej.parent || node.parent == nodej)
            return (nodej.y < node.y  && node.y  < nodej.y+nodej.height) ||
                   (node.y  < nodej.y && nodej.y < node.y +node.height );

        bool nodejSurroundsNode = nodej.parent.y < node.y+node.height   && nodej.y+nodej.height > node.y ;
        bool nodeSurroundsNodej = node.parent.y  < nodej.y+nodej.height && node.y +node.height  > nodej.y;
        return nodejSurroundsNode || nodeSurroundsNodej;
    }

    void UpdateNodePositions(List<Node> nodes)
    {
        float avgX = 0;
        foreach(Node node in nodes) avgX += node.x / (float)nodes.Count;

        foreach(Node node in nodes)
        {
            Node parent = node.parent;
            
            float parentPullForce = Mathf.Min(Mathf.Sign(parent.x-node.x) * NODE_WIDTH, parent.x - node.x);
            float childPullForce = 0;
            float divisor = strongChildPull ? 1 : node.children.Count;
            foreach(Node c in node.children) childPullForce += Mathf.Min(Mathf.Sign(c.x-node.x) * NODE_WIDTH, c.x - node.x); // changed from test script

            float force = parentPullForce+childPullForce;
            float newX = ((node.x + FORCE_DAMPENING*force) + (node.x)) / 2f;

            foreach(Node nodej in nodes)
            {
                if (node == nodej) continue;
                if (!CouldCollide(node, nodej)) continue;

                // if these nodes will collide
                if (Mathf.Abs(newX - nodej.x) < NODE_WIDTH*BUFFER_FACTOR || Mathf.Sign(node.x - nodej.x) != Mathf.Sign(newX - nodej.x))
                {
                    newX = node.x > nodej.x 
                        ? nodej.x + NODE_WIDTH*BUFFER_FACTOR 
                        : nodej.x - NODE_WIDTH*BUFFER_FACTOR;
                }
            }

            node.x = newX;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        List<SpeciesRecord> species = new List<SpeciesRecord>();
        List<Node> nodes = new List<Node>();
        Dictionary<int, Node> nodesById = new Dictionary<int, Node>();

        foreach (string file in Directory.EnumerateFiles(folderPath, "*.json"))
        {
            string contents = File.ReadAllText(file);
            SpeciesRecord s = UnityEngine.JsonUtility.FromJson<SpeciesRecord>(contents);
            species.Add(s);

            Node n = new Node() 
            {
                x = 0,
                y = s.generation,
                species = s,
                height = NODE_HEIGHT*BUFFER_FACTOR
            };
            nodes.Add(n);
            nodesById[n.species.speciesId] = n;
        }

        foreach(Node n in nodes)
        {
            n.parent = nodesById[n.species.parent_species_id];
            n.parent.children.Add(n);
        }

        Node root = nodesById[0];

        for(int i = 0; i < 100; i++) UpdateNodePositions(nodes);

        // TODO: NOW CREATE LINE RENDERERS LIKE SO:
        /*    p ---
         *          \
         *           |
         *           |
         *           |
         *           |
         *           |
         *           c
         */
        // TODO: NOW RENDER EACH PLANT CONTAINED IN EACH NODE
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
