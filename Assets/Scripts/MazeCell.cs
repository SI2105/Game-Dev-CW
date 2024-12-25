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

    public void MarkAsRoom()
    {
        IsRoom = true;
        ClearAllWalls(); // Open the room completely
    }

   public Dictionary<string, GameObject> GetWalls()
    {
        Dictionary<string, GameObject> activeWalls = new Dictionary<string, GameObject>();

        if (_leftWall.activeSelf)
        {
            activeWalls["leftWall"] = _leftWall;
        }

        if (_rightWall.activeSelf)
        {
            activeWalls["rightWall"] = _rightWall;
        }

        if (_frontWall.activeSelf)
        {
            activeWalls["frontWall"] = _frontWall;
        }

        if (_backWall.activeSelf)
        {
            activeWalls["backWall"] = _backWall;
        }

        return activeWalls;
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

    public void ClearLeftWall()
    {
        _leftWall.SetActive(false);
    }

    public void ClearRightWall()
    {
        _rightWall.SetActive(false);
    }

    public void ClearFrontWall()
    {
        _frontWall.SetActive(false);
    }

    public void ClearBackWall()
    {
        _backWall.SetActive(false);
    }
}
