using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public Terrain terrain; // The terrain object to modify
    public TerrainLayer terrainLayer; // Terrain layer to apply
    public Texture2D grassTexture; // Grass texture to apply

    // Prefabs for trees, shrubs, and rocks
    public GameObject treePrefab;
    public GameObject shrubPrefab;
    public GameObject rockPrefab;

    // Independent public variables for counts
    public int numberOfTrees = 100;
    public int numberOfShrubs = 50;
    public int numberOfRocks = 30;

    public GameObject mazeCellPrefab; // MazeCell object to avoid overlapping

    void Start()
    {
        if (terrain == null || terrainLayer == null || grassTexture == null)
        {
            Debug.LogError("Please assign the Terrain, TerrainLayer, and GrassTexture.");
            return;
        }

        Debug.Log("Starting TerrainManager setup...");

        ApplyTerrainLayer();
        BrushGrassAcrossTerrain();
        PlaceObjectsWithCollisionCheck();
    }

    private void ApplyTerrainLayer()
    {
        TerrainData terrainData = terrain.terrainData;

        Debug.Log("Replacing terrain layers...");
        terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
        Debug.Log("Terrain layer replaced successfully.");
    }

    private void BrushGrassAcrossTerrain()
    {
        TerrainData terrainData = terrain.terrainData;

        Debug.Log("Creating grass DetailPrototype...");
        DetailPrototype grassDetail = new DetailPrototype
        {
            prototypeTexture = grassTexture,
            renderMode = DetailRenderMode.GrassBillboard,
            minWidth = 1f,
            maxWidth = 2f,
            minHeight = 0.5f,
            maxHeight = 1f,
            healthyColor = Color.green,
            dryColor = Color.yellow,
            noiseSpread = 0f, // Ensures grass is placed uniformly
            density = 50, // Adjust density here for desired coverage
            noiseSeed = 0 // Removes randomness for consistent coverage
        };

        Debug.Log($"Grass DetailPrototype created with density: {grassDetail.density}");

        Debug.Log("Replacing detail prototypes...");
        terrainData.detailPrototypes = new DetailPrototype[] { grassDetail };

        // Set the detail resolution
        int detailResolution = terrainData.detailResolution;
        int[,] grassMap = new int[detailResolution, detailResolution];

        Debug.Log("Populating grass detail layer...");
        for (int x = 0; x < detailResolution; x++)
        {
            for (int y = 0; y < detailResolution; y++)
            {
                grassMap[x, y] = Mathf.FloorToInt(grassDetail.density / 2); // Use density as a base for the map
            }
        }

        Debug.Log("Applying grass detail layer across the terrain...");
        terrainData.SetDetailLayer(0, 0, 0, grassMap);

        terrain.terrainData.RefreshPrototypes();
        Debug.Log("Grass texture applied successfully and uniformly.");
    }

    private void PlaceObjectsWithCollisionCheck()
    {
        TerrainData terrainData = terrain.terrainData;

        if (treePrefab == null || shrubPrefab == null || rockPrefab == null)
        {
            Debug.LogWarning("Please assign Tree, Shrub, and Rock prefabs.");
            return;
        }

        Debug.Log("Adding tree prototypes...");
        TreePrototype[] treePrototypes = new TreePrototype[3];
        treePrototypes[0] = CreateTreePrototype(treePrefab);
        treePrototypes[1] = CreateTreePrototype(shrubPrefab);
        treePrototypes[2] = CreateTreePrototype(rockPrefab);
        terrainData.treePrototypes = treePrototypes;

        Debug.Log("Placing trees, shrubs, and rocks with collision checks...");

        // Temporary list to hold all tree instances
        var treeInstances = new System.Collections.Generic.List<TreeInstance>();

        // Place objects
        PlaceSpecificObjects(treeInstances, numberOfTrees, 0, treePrefab, terrainData);
        PlaceSpecificObjects(treeInstances, numberOfShrubs, 1, shrubPrefab, terrainData);
        PlaceSpecificObjects(treeInstances, numberOfRocks, 2, rockPrefab, terrainData);

        // Apply tree instances to the terrain
        terrainData.treeInstances = treeInstances.ToArray();
        Debug.Log("All objects placed successfully.");
    }

    private void PlaceSpecificObjects(System.Collections.Generic.List<TreeInstance> treeInstances, int number, int prototypeIndex, GameObject prefab, TerrainData terrainData)
    {
        int placedObjects = 0;

        while (placedObjects < number)
        {
            // Randomize position
            float randomX = Random.value;
            float randomZ = Random.value;
            Vector3 worldPosition = new Vector3(
                randomX * terrainData.size.x,
                0,
                randomZ * terrainData.size.z
            );

            // Get terrain height at position
            worldPosition.y = terrain.SampleHeight(worldPosition) + terrain.transform.position.y;

            // Check for MazeCell overlap
            if (Physics.CheckSphere(worldPosition, 2f, LayerMask.GetMask("MazeCell")))
            {
                continue; // Skip this position if it overlaps with a MazeCell
            }

            // Create a new TreeInstance
            TreeInstance treeInstance = new TreeInstance
            {
                position = new Vector3(randomX, 0, randomZ), // Normalized position
                prototypeIndex = prototypeIndex,
                widthScale = Random.Range(0.8f, 1.2f),
                heightScale = Random.Range(0.8f, 1.2f),
                color = Color.white,
                lightmapColor = Color.white
            };

            // Add to the list
            treeInstances.Add(treeInstance);
            placedObjects++;
        }

        Debug.Log($"Successfully placed {placedObjects} objects of type {prefab.name}.");
    }

    private TreePrototype CreateTreePrototype(GameObject prefab)
    {
        TreePrototype treePrototype = new TreePrototype
        {
            prefab = prefab
        };
        return treePrototype;
    }
}
