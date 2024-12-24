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

    public List<GameObject> getWall()
{
    List<GameObject> activeWalls = new List<GameObject>();

    if (_leftWall.activeSelf)
    {
        activeWalls.Add(_leftWall);
    }

    if (_rightWall.activeSelf)
    {
        activeWalls.Add(_rightWall);
    }

    if (_frontWall.activeSelf)
    {
        activeWalls.Add(_frontWall);
    }

    if (_backWall.activeSelf)
    {
        activeWalls.Add(_backWall);
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
