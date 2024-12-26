using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell : MonoBehaviour
{
    [SerializeField]
    private GameObject _leftWall;

    [SerializeField]
    private GameObject _rightWall;

    [SerializeField]
    private GameObject _frontWall;

    [SerializeField]
    private GameObject _backWall;

    [SerializeField]
    private GameObject _ceiling;

    [SerializeField]
    private GameObject ceilingPrefab;

    [SerializeField]
    private GameObject _unvisitedBlock;

    [SerializeField]
    private GameObject floorObject; // The initial floor object

    [SerializeField]
    private GameObject[] tilePrefabs; // Array of random tile prefabs

    public bool IsVisited { get; private set; }
    public int GridX { get; set; } // Store the grid X index
    public int GridZ { get; set; } // Store the grid Z index
    public bool IsRoom { get; private set; }
    public bool IsRoomEntrance { get; private set; }

    // Dictionary to track archway status for each wall
    private Dictionary<string, bool> _archwayStatus = new Dictionary<string, bool>();

    public void MarkAsRoomEntrance()
    {
        IsRoomEntrance = true;
    }

    public void MarkAsRoom()
    {
        IsRoom = true;
        ClearAllWalls(); // Open the room completely
    }

    public void Visit()
    {
        IsVisited = true;
        _unvisitedBlock.SetActive(false);

        if(!IsRoom){
             ReplaceFloorWithTile(); // Replace floor with a random tile when the cell is visited
            
        }
         ReplaceCeilingWithTile(); // Replace ceiling with a tile when the cell is encountered
    }

 
    // Replace the ceiling object with a single random tile
    private void ReplaceCeilingWithTile()
    {
        if (_ceiling == null) return;

        // Store the position and size of the current ceiling object
        Vector3 ceilingPosition = _ceiling.transform.position;
        Vector3 ceilingScale = _ceiling.transform.localScale;

        // Adjust the scale for the new ceiling tile
        Vector3 adjustedScale = new Vector3(ceilingScale.x / 2.5f, 1f, ceilingScale.z / 2.5f);

        // Destroy the existing ceiling object
        Destroy(_ceiling);

        // Instantiate the ceilingPrefab at the original position
        GameObject newCeiling = Instantiate(ceilingPrefab, ceilingPosition, Quaternion.identity, this.transform);

        // Adjust the scale, preserving the prefab's y scale
        Vector3 tileScale = newCeiling.transform.localScale;
        newCeiling.transform.localScale = new Vector3(adjustedScale.x, tileScale.y, adjustedScale.z);

        // Adjust the position relative to the parent, offset by -0.5 in the z-axis
        newCeiling.transform.localPosition += new Vector3(0, 0, -0.5f);

          // Apply a -180° rotation on the Z-axis after instantiation
        newCeiling.transform.Rotate(0, 0, -180f);
    }

   // Replace the floor object with a single random tile
    private void ReplaceFloorWithTile()
    {
        if (floorObject == null) return;

        // Store the position and size of the current floor object
        Vector3 floorPosition = floorObject.transform.position;
        Vector3 floorScale = floorObject.transform.localScale;

        Vector3 adjustedScale = new Vector3(floorScale.x / 4.8f, 1f, floorScale.z / 4.8f); // Adjust x and z, leave y as default

        // Destroy the existing floor object
        Destroy(floorObject);

        // Choose a random tile prefab
        GameObject randomTilePrefab = GetRandomTile();

        // Instantiate the new tile at the same position
        GameObject newTile = Instantiate(randomTilePrefab, floorPosition, Quaternion.identity, this.transform);

        // Apply the adjusted scale, preserving the prefab's y scale
        Vector3 tileScale = newTile.transform.localScale;
        newTile.transform.localScale = new Vector3(adjustedScale.x, tileScale.y/3f, adjustedScale.z);
    }


    // Get a random tile prefab
    private GameObject GetRandomTile()
    {
        int randomIndex = Random.Range(0, tilePrefabs.Length);
        return tilePrefabs[randomIndex];
    }

    public Dictionary<string, (GameObject wallObject, bool isArchway)> GetWalls()
    {
        Dictionary<string, (GameObject, bool)> walls = new Dictionary<string, (GameObject, bool)>
        {
            { "leftWall", (_leftWall, IsArchway("leftWall")) },
            { "rightWall", (_rightWall, IsArchway("rightWall")) },
            { "frontWall", (_frontWall, IsArchway("frontWall")) },
            { "backWall", (_backWall, IsArchway("backWall")) }
        };

        return walls;
    }

    public void ClearAllWalls()
    {
        ClearFrontWall();
        ClearBackWall();
        ClearLeftWall();
        ClearRightWall();
    }

    public void ClearLeftWall()
    {
        _leftWall.SetActive(false);
        MarkAsArchway("leftWall");
    }

    public void ClearRightWall()
    {
        _rightWall.SetActive(false);
        MarkAsArchway("rightWall");
    }

    public void ClearFrontWall()
    {
        _frontWall.SetActive(false);
        MarkAsArchway("frontWall");
    }

    public void ClearBackWall()
    {
        _backWall.SetActive(false);
        MarkAsArchway("backWall");
    }

    private void MarkAsArchway(string wallName)
    {
        if (_archwayStatus.ContainsKey(wallName))
        {
            _archwayStatus[wallName] = true;
        }
        else
        {
            _archwayStatus.Add(wallName, true);
        }
    }

    private bool IsArchway(string wallName)
    {
        return _archwayStatus.ContainsKey(wallName) && _archwayStatus[wallName];
    }
}
