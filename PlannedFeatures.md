For display only - use the NEAT strategy for a speciation histogram, the same graph as in my JavaNEAT program


**Cladogram**
*Status:* In Progress
☑ read in species files
☑ algorithm to properly place nodes
☐ render plants at correct nodes
☐ export png
☐ export gif with plants rotating in place
☐ better renderer (not for use in simulation)
☐ generate genus and species names

*Dev Notes:*
Track evolutionary history (should be pretty easy with asexual reproduction only, or could implement the same speciation strategy as above (again, species are for display only, and not used in computation).
Generate species and genus names using either the below strategy or tries, using a dictionary of genus names and separately a dictionary of species names
https://jdifebo.github.io/WordMaker/latin.html
List of (almost) all species https://www.uniprot.org/docs/speclist.txt
The trie method would work like this: for each string in the dictionary, iterate from 0 to n-(markovOrder+1)
At each iteration, insert substring(i, i+markovOrder+1) into the trie
Done!


**More properties of terrain**
*Summary:* add more properties like fertility, where has a different effect
*Status:* In Planning
*Dev Notes:*
Idea: water = reduced penalty for leaf area, nutrient = # of growth iterations, temperature = # of symbols added per iteration


**Simulate Herbivores**
*Summary:* add a simulation step where all leaf meshes below a given height have a chance to be removed before sunlight exposure testing
*Status:* In Planning
*Dev Notes:*
Idea: remove all foliage below a certain height before evaluation - simulate herbivores
To prevent plants from just having a hard line where leaves don’t grow below, introduce randomness. The lower a leaf is, the more likely it is to be removed
No direct fitness penalty (though this does happen before fitness is evaluated - plants are better off using their limited resources to grow higher leaves)


**Multiple stages of growth**
*Summary:* enable plants to grow during evaluation
*Status:* In Planning
*Dev Notes:*
This would work by re-applying rules to all agents’ sentences, and then re-rendering and re-evaluating viability before finally creating seeds for the next generation. An agent’s final viability will be the product of its viability in each iteration


**Healing**
*Summary:* Force plants to evolve ability to heal - requires "Multiple stages of growth" 
*Status:* In Planning
*Dev Notes:*
randomly damage plants so that they have to evolve the ablity to heal and recover


**Parametric L Systems**
*Summary:* Add metadata to each token that can be evolved and effected by applied rules
*Status:* In Planning
*Dev Notes:*
Each token will no longer be a single character, but instead have metadata - represented in string form like so: A(meta1,meta2,)
Rules will be allowed to evolve simple equations in each meta slot, eg the following LSystem
AC
A(x)->B(1)
B(x)->D(2*x)
C(1)->E
would create
D(2)C

And
A(1)A(2)CA(1)A(1)
A(x)A(x+1)->B(x+2)
would create
B(3)CA(1)A(1)

The first parameter of stem characters would be used by the modeler to determine the length of the stem,
The second parameter of stem characters would control thickness at the base. However, all stems’ thicknesses will be limited to the thickness of its parent at most
the first parameter of turn characters would be used to determine turn amount
Note: the modeler would have to be changed to determine stem radius by calculating the total LENGTH of stem above it rather than the total NUMBER of stems above it


**Phototaxis**
*Summary:* Enable plants to grow towards light. Requires Parametric L Systems and multiple stages of growth and evaluation
*Status:* In Planning
*Dev Notes:*
add a parameter to < symbols that stores the amount of light that leaf recieved, updated by the environment and not the genes


**Improved Context Sensitivity**
*Summary:* Add tokens to go in rules' LHS to improve the context sensitivity of the LSystems
*Status:* In Planning
*Dev Notes:*
Regex like tokens for rules’ LHS - important symbols: a “not group”, a (.*) symbol (use the unicode … symbol)


**Wind**
*Summary:* Wind destabilizes unadapted plants and biases seed distribution
*Status:* In Planning
*Dev Notes:*
simulate wind - a force is applied to all plants’ leaves as the projection of the wind vector onto the normal vector of the leaf (or the negative normal if it’s pointing into the wind). Multiply this force by the square of the area the width of the leaf. Sum all the forces together for a given plant, and shift its center of gravity by adding the resultant vector to its normal center of gravity. Proceed with penalizing instability.


**Roots for resource collection**
*Summary:* The amount of resources available to a plant would be determined by its roots system as well as the ground it's rooted in
*Status:* Building Idea
*Dev Notes:*
Roots for resource collection? Stems below ground would be considered roots


**Flowers**
*Summary:* Allow plants to evolve flowers - both for looks and to eventually enable sexual reproduction
*Status:* In Planning
*Dev Notes:*
Flowers - colored leaves (leaf parameter or extra token present inside leaf brackets?)
Prettier flowers should require more symbols but increase reproduction chance more than less pretty flowers
Maybe all petals (leaves) are naturally green, but can slowly chance their color by adding more and more color symbols inside the leaf structure
Eg &lt;r\] would be sliiightly red but still mostly green, while &lt;rrrrr\] would be neon red
More structurally complex flowers should increase reproduction chance more than less complex flowers
This way, better flowers mean better fitness, but require more resources - this allows the simulation to both reward AND penalize nice flowers
Flowers should also get a bonus for sun exposure, to encourage plants to display their flowers
Gradients? &lt;r\[Ag\]Bg\] makes petal red at the base, fade to g at the edge? Use pipe symbol to say “abrupt color change”?
Add bioluminescence colors - cyan and green, cause the flower to glow. If any bioluminescence is present, the similarity calculation uses only the bioluminescence and ignores the regular color (this flower is assumed to be nocturnal)


**Cross Breeding**
*Summary:* Sexual reproduction, should lead to faster / better evolution (requires "Flowers")
*Status:* In Planning
*Key Issue:* Need method to determine "similarity" between two flower models
*Dev Notes:*
Cross-breeding (sexual reproduction)? Requires flowers
The more similar two flowers are, the more likely they’ll be chosen for cross breeding
This way two species that have similar flowers will die out - all of their chances for reproduction will be wasted on incorrect pairings, producing non-viable sickly offspring
Seeds without flowers are not cross-bred
To encourage some plants to reproduce asexually, also penalize # of flowers. This way, once one plant evolves complex flowers, in order to compete, other plants will have to not use flowers. Their crummy flowers wont be enough to overcome the penalty
Note: a flower is defined as a leaf containing a seed <...*...]
To help plants not self-polinate, add a “polination window”, which represents the time of year flowers bloom and are available for pollination. It’s controlled by four symbols anywhere within the flower: two for increasing/decreasing the size of the window, and two for moving the start of the window. Each flower on the same plant can have different windows. Likelihood for one flower to be polinated by another is multiplied by the fraction of its window the other’s window covers
Note: the polination routine works by listing all flowers once, and then listing the liklihood for every other flower to polinate it, and then for each flower, choosing the most likely polinator


**Fruit**
*Summary:*
*Status:* Building Idea
*Dev Notes:*
Allows plant to add layers to the seed, including bark and/or fruit
Fruit layers increase distance seeds can go but also increase chance a seed is a dud (because it gets eaten) - more fruit layers, more distance
Bark layers increase chance that seed will sprout instead of being a dud

No need for a special “open seed” and “close seed” symbols. If flowers are also implemented, since a leaf with a seed inside is considered a flower, we can apply all bark and fruit within that flower to the seeds that also belong in that flower.
eg <*#$\] makes a seed with one layer of bark and one layer of fruit


**Bark**
*Summary:* Allow plants to evolve bark to improve their stability without changing their structure
*Status:* In Planning
*Dev Notes:*
What would it do?
1. Improve the stability score by bringing the x and z components of the portion of the center of gravity contributed by symbols up to N after it closer to its own x and z value
2. Decrease chance of ransom damage
3. Naturally decreases fitness by being an extra non-leaf symbol
4. More bark symbols in a row increases effect, to a point
5. Increase leaf toughness but decrease efficiency (if applied to leaf via #< where # is the bark symbol)
6. Increase the diameter of the vertex it’s applied to
Note: the string #AB applies bark to A, but not B
bark stretches backwards. May make more realistic plants. Ex:
AB#C applies bark to A and B but not C
A#BC#D applies two layers to A
AB\[C\]D# applies bark to AB and D, not C
AB\[C#\]D applies to AB and C, not D

Leaves are special
AB&lt;C\[D\]E#\]F applies bark only to the leaf CDE, not branches A, B or F
Also applies bark to branches CDE
AB&lt;C\[D#\]E\]F still applies bark to whole leaf CDE
Also applies bark to branches CD but not E

Plants are penalized for having too much bark, and for having bark too high up

New way to handle bark: I like the way the symbol applies bark to all portions of stem below it, however I think everything else should be changed
During the center of mass calculation, all stems with bark should have their x and z coords multiplied by (1+barkCount*0.1) where 0.1 is a constant adjustable from the UI. All stems will also have their x and z coords multiplied by (1/(parentBarkCount*0.1+1))
Therefore stems with bark are heavier, but support their children

Finally, once bark is implemented, the green-to-brown coloration should be determined at the top by the bark level and the bottom by the parent’s bark level, rather than by # of stems supported.


**Sun sensitivity**
*Summary:* Implement leaves burning in the sun - something that happens in real life
*Status:* In Planning
*Dev Notes:*
Could combine bark idea with sun sensitivity - if a plant gets too much sun on a given leaf, it’s penalized. Bark on leaves increases the amount of sun a leaf can take before penalty. Hopefully this will encourage evolution of trees that like full sun and short plants that require partial shade
These two ideas both require that each leaf gets its own color in the sun rendering stage - not just each plant
Could be done pretty easily by assigning each plant a unique R (or even RG) and each leaf a B that’s unique to that plant but not universally unique


**Generating Plants Across a Generated Planet**
*Summary:* Evolve a whole planet's worth of plant species at once
*Status:* In Planning
*Dev Notes:*
To generate plants for a given planet map:
1. First add a nutrient value to the map - blown from mountains and tiles with > 0.1 average rainfall (deserts) (falls out of the sky and lands when wind slows or enough gets put into a single tile’s sky so it gets saturated) bonus fertility for being next to rivers
1. Nutrient that lands in the ocean is moved by ocean currents
2. Simulate weather for around 50 to 60 steps
3. Create a grid over the planet. Make this grid relatively large, around 4 squares per region, maybe less.
4. Sample water (wetness), fertility, and temperature (air temperature) for each grid point by taking the value of the nearest region (works because voronoi)
5. Incorporate elevation when considering seed placement - consider tiles with a higher elevation to be further away than they really are and lower tiles to be closer
6. Bonus: Incorporate wind when distributing seeds - tiles in the direction of the wind are closer, against the wind are farther
Now when a world is scaled up for in person exploration, seed each region with a ton of plant species that evolved in a tile corresponding to that region
So during generation, the world will have around 50 trees, 3 per region. When generating it for in person use, it’ll have 5000 trees, replicas of species created during generation

notes from duplicate idea:
First, generate a map using my world generator
Next, chunk it into a grid
Add code to invalidate any seed that lands in the ocean (unless it has evolved seafaring genes? Then move it to a shore according to the currents? Something something for wind spread seeds?)
Then, if you want to visit any place on the map in first person, it can be generated by selecting a few plants that evolved there, consider them species, and place a bunch of copies in the terrain generated for that place


**Structural Roots**
*Summary:* Allow plants to evolve structures like prop roots
*Status:* In Planning
*Dev Notes:*
Anywhere a stem from the plant intersects the ground is considered another root point - when calculating stability, multiple centers of mass are calculated. Each stem contributes to the center of mass point belonging to the root point it’s closest to. This allows prop roots (like the elephant plant) and runners (like in grass) to evolve


**Symmetry**
*Summary:* Allow plants to easily evolve repeatably symmetrical structures
*Status:* In Planning
*Dev Notes:*
An issue with the original paper is that all leaves were asymmetrical - they grew on one side of the stem only
Idea: introduce a new symbol, {
This means “repeat enclosing structure twice, and on the second time, do it mirrored” - mirroring is simple, all that needs to happen is whenever a \\ symbol is found, treat it as a / symbol. Other types of symmetry can be accomplished by inverting different subsets of rotation symbols


**Radial Symmetry**
*Summary:* Real plants are known to build spiral structures. Is this easily doable in the current system or do we need to implement some sort of radial symmetry?
*Status:* Building idea
*Dev Notes:*
Add a number in front of the squiggle bracket to return to it that many times and increment 360/(times+1)
Repeat without returning? ie {2A+B} would expand out to A+BA+B instead of \[A+B\]\[A+B\]


**Evolution in terrainy terrain**
*Summary:* Evolve plants in realistic terrain
*Status:* In Planning
*Dev Notes:*
https://youtu.be/eaXk97ujbPQ
An interesting thing about using this terrain is that the flat valleys can be given a high fertility value while the stoney peaks get a low value - this means plants get either lots of sun or lots of fertility, leading to two very different environments right next to eachother!
Calculate fertility at a given point by (newHeight - oldHeight)
1. Incorporate elevation when considering seed placement - consider tiles with a higher elevation to be further away than they really are and lower tiles to be closer
2. Bonus: Incorporate wind when distributing seeds - tiles in the direction of the wind are closer, against the wind are farther Higher elevations experience stronger wind


**Diploidy**
*Summary:* Give plants two sets of genes, like real life
*Status:* In Planning
*Dev Notes:*
Rules are easy, simply use all the rules from both haploid genomes at the same time.
For axioms it’s a little trickier. I think I should use LCS: https://en.m.wikipedia.org/wiki/Longest_common_subsequence_problem
It measures the edit distance between two strings allowing only for insertions and deletions, is relatively fast, and is used in real bioinformatics.
I’ve got two ideas for how to use it
1. the interpreted genome’s axiom is the LCS of the two axioms
2. The interpreted genome’s axiom is the LCS with each insertion and deletion from the two genomes randomly chosen for inclusion or exclusion
Number 1 sounds better, more reliable. 2 is a bit TOO random
When reproducing, the genes should be shuffled in several ways. For each parent, 1. Rules get paired up like chromosomes, using innovation numbers and a random member of each pair is chosen for the offspring. 2. Rarely, crossover occurs before a chromosome is chosen from each pair


**Async generations**
*Summary:* Allow plants to evolve lifespans
*Status:* Building Idea
*Dev Notes:*
Give some proportion of the population a nonmutatable gene that makes them survive for 2 generations and therefore only seed every other generation


**Specific Simulations**
*Summary:* 
*Status:* In Planning
*Dev Notes:*
1. A simulation mode specifically for generating trees
    Has a viability component for average leaf height - the higher the average leaf height, the higher the viability
    Leaf opacity will need to be decreased for this simulation, maybe even down to 25%


**Realtime simulation**
*Summary:* Replace the concept of "generations" with "time steps" (does not require any other features)
*Status:* In Planning
*Dev Notes:*
Instead of each step being a generation, simply evaluate on each step
Every agent gets one round of rules being applied to their sentences (then their models get regenerated).
Then viability is calculated.
Viability is divided by # of symbols in current sentence
Seeds are dispersed, and seed symbols are removed from agent sentences
If a seed lands on another plant and its viability is higher, the plant dies. Otherwise the seed is deleted
When a seed lands, it does a density check. If any of its neighbors have a density over the limit, then that neighbor and the seed compare viability. Whichever is higher survives. if the seed’s location is still over density, every agent contributing to that density score is compared to the seed, in a random order, until either the seed dies or the density is not above the limit
