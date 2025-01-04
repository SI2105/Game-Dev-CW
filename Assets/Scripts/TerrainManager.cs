using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    // Public variables to set tile prefabs and the terrain in the inspector
    public GameObject[] tilePrefabs; // Array of tile prefabs to randomly choose from
    public GameObject plainTilePrefab; // The plain tile prefab
    public Terrain terrain; // Terrain object to determine terrain dimensions
    public float plainTileWeight = 1f; // Weight for the plain tile (between 0 and 1)

    void Start()
    {
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        // Get terrain dimensions
        float terrainWidth = terrain.terrainData.size.x;
        float terrainLength = terrain.terrainData.size.z;
        float tileSize = plainTilePrefab.GetComponent<Renderer>().bounds.size.x;

        // Place tiles over the terrain
        for (float x = 0; x < terrainWidth; x += tileSize)
        {
            for (float z = 0; z < terrainLength; z += tileSize)
            {
                Vector3 position = new Vector3(x, terrain.transform.position.y + 0.1f, z);
                GameObject tileToPlace = GetRandomTile();
                Instantiate(tileToPlace, position, Quaternion.identity, this.transform);
                tileToPlace.SetActive(false);
            }
        }
    }

    private GameObject GetRandomTile()
    {
        // Use a weighted random selection to choose between plainTilePrefab and other tilePrefabs
        float randomValue = Random.Range(0f, 1f);
      
        if (randomValue <= plainTileWeight)
        {
            return plainTilePrefab;
        }
        else
        {
             Debug.LogError("Using random tile.");
            int randomIndex = Random.Range(0, tilePrefabs.Length);
            return tilePrefabs[randomIndex];
        }
    }
}
