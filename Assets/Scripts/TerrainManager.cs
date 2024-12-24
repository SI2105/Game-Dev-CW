using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    // Public variables to set the tile and wall prefab in the inspector
    public GameObject tilePrefab; // Tile prefab for terrain floor
    public Terrain terrain; // Terrain object to determine terrain dimensions
    void Start()
    {

        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        // Get terrain dimensions
        float terrainWidth = terrain.terrainData.size.x;
        float terrainLength = terrain.terrainData.size.z;
        float tileSize = tilePrefab.GetComponent<Renderer>().bounds.size.x;

        // Place tiles over the terrain
        for (float x = 0; x < terrainWidth; x += tileSize)
        {
            for (float z = 0; z < terrainLength; z += tileSize)
            {
                Vector3 position = new Vector3(x, terrain.transform.position.y+0.1f, z);
                Instantiate(tilePrefab, position, Quaternion.identity, this.transform);
            }
        }
    }

}
