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
    private GameObject _unvisitedBlock;

    public bool IsVisited { get; private set; }
    public int GridX { get; set; } // Store the grid X index
    public int GridZ { get; set; } // Store the grid Z index
    public bool IsRoom { get; private set; }
    public bool IsRoomEntrance { get; private set; }

    public void MarkAsRoomEntrance()
    {
        IsRoomEntrance = true;
    }
    
    // Dictionary to track archway status for each wall
    private Dictionary<string, bool> _archwayStatus = new Dictionary<string, bool>();

    public void MarkAsRoom()
    {
        IsRoom = true;
        ClearAllWalls(); // Open the room completely
    }

    // Modified GetWalls to return all walls, including their archway status
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

    public void Visit()
    {
        IsVisited = true;
        _unvisitedBlock.SetActive(false);
    }

    // Clear and mark as archway
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

    // Mark a wall as an archway
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

    // Check if a wall is an archway
    private bool IsArchway(string wallName)
    {
        return _archwayStatus.ContainsKey(wallName) && _archwayStatus[wallName];
    }
}
