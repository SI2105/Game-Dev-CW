using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.AI.Navigation; // For NavMeshSurface


public class PCG_Generator : MonoBehaviour
{


    public GameObject floorParent;

    public NavMeshSurface navMeshSurface;

    [SerializeField]
    private Wall _wall;

    
    [SerializeField]
    private GameObject roomPrefab;

    [SerializeField]
    private GameObject _archwayPrefab;

    [SerializeField]
    private GameObject _archwayWithDoorPrefab;

    [SerializeField]
    private GameObject _plainWallPrefab;

    [SerializeField]
    private int _roomBufferSize = 2; // Minimum buffer between rooms  


    public Transform enemy;

    [SerializeField]
    private float _maze_configuration = 6f; // Minimum buffer between rooms    

    [SerializeField]
    private int _roomCount = 3; // How many rooms to generate

    [SerializeField]
    private int _minRoomSize = 2; // Minimum room size

    [SerializeField]
    private int _maxRoomSize = 4; // Maximum room size

    private List<Room> _rooms = new List<Room>(); // Store all the rooms

    [SerializeField]
    private MazeCell _mazeCellPrefab;

    [SerializeField]
    private int _mazeWidth;

    [SerializeField]
    private int _mazeDepth;

    public MazeCell[,] _mazeGrid;

    private List<GameObject> allObjects = new List<GameObject>();
    public float activationRadius = 50f; // Radius around the player for activation
    public Transform playerTransform; // Reference to the player's transform

  
    [SerializeField]
    public Terrain terrain;

    private float cellSizeX;

    private float cellSizeZ;

    public GameObject zombiePrefab; // Zombie prefab
    public float zombieSpawnRadius = 20f; // Radius for spawning zombies in cells
    public float zombieDespawnRadius = 25f; // Radius for despawning zombies
    public int maxZombies = 15; // Maximum number of zombies
    public float zombieSpawnInterval = 60f; // Time interval between spawns
    public float level;
    private List<GameObject> activeZombies = new List<GameObject>(); // Track active zombies
    private float zombieSpawnTimer = 0f; // Timer for spawn control



    void Update(){
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue; // Skip if object is destroyed

            float distanceToPlayer = Vector3.Distance(playerTransform.position, obj.transform.position);

            // Activate objects within the radius, deactivate objects outside
            if (distanceToPlayer <= activationRadius)
            {
                if (!obj.activeSelf) // Only activate if not already active
                {
                    obj.SetActive(true);
                }
            }
            else
            {
                if (obj.activeSelf) // Only deactivate if currently active
                {
                    obj.SetActive(false);
                }
            }
        }

        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                MazeCell cell =_mazeGrid[x, z];
              

                GameObject floor = cell.floorObj;

                if (floor == null) continue; // Skip if object is destroyed
               
                float distanceToPlayerFloor = Vector3.Distance(playerTransform.position, floor.transform.position);

                    // Activate objects within the radius, deactivate objects outside
                if (distanceToPlayerFloor <= activationRadius)
                {
                    if (!cell.floorObj.activeSelf) // Only activate if not already active
                    {
                        cell.floorObj.SetActive(true);
                        cell.ceilingObject.SetActive(true);
                    }
                }
                else
                {
                    if (cell.floorObj.activeSelf) // Only activate if not already active
                    {
                        cell.floorObj.SetActive(false);
                        cell.ceilingObject.SetActive(false);
                    }
                }
                
            }
        }


        // Update zombie spawn timer
        zombieSpawnTimer += Time.deltaTime;

        // Spawn zombies if the timer reaches the interval and the max limit isn't reached
        if (zombieSpawnTimer >= zombieSpawnInterval && activeZombies.Count < maxZombies)
        {
            Debug.Log("Spawning zombies");
            SpawnZombiesInMazeCellsAroundPlayer();
            zombieSpawnTimer = 0f; // Reset the spawn timer
        }

        // Check for zombies outside the despawn radius and destroy them
        for (int i = activeZombies.Count - 1; i >= 0; i--)
        {
            GameObject zombie = activeZombies[i];
            float distanceToPlayer = Vector3.Distance(playerTransform.position, zombie.transform.position);

            if (distanceToPlayer > zombieDespawnRadius)
            {
                Destroy(zombie); // Destroy the zombie
                activeZombies.RemoveAt(i); // Remove from the list
            }
        }

    }

    private void SpawnZombiesInMazeCellsAroundPlayer()
    {
        // Get valid cells within the radius
        List<MazeCell> validCells = GetMazeCellsWithinRadius(playerTransform.position, zombieSpawnRadius);

        Debug.Log(validCells.Count);

        if (validCells.Count == 0)
            return; // No valid cells found

        // Determine the number of cells to pick (maximum 4 or total valid cells, whichever is smaller)
        int cellsToPick = Mathf.Min(4, validCells.Count);

        // Randomly select cells from the valid cells
        List<MazeCell> selectedCells = new List<MazeCell>();
        for (int i = 0; i < cellsToPick; i++)
        {
            int randomIndex = Random.Range(0, validCells.Count);
            selectedCells.Add(validCells[randomIndex]);
            validCells.RemoveAt(randomIndex); // Remove to avoid selecting the same cell again
        }

        // Spawn 4-6 zombies in each selected cell
        foreach (MazeCell cell in selectedCells)
        {
            int zombiesToSpawnInCell = 1;
            for (int i = 0; i < zombiesToSpawnInCell; i++)
            {
                Vector3 spawnPosition = cell.transform.position + GetRandomOffset(); // Slightly randomize the spawn position
                GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
                MiniEnemyAIController miniEnemy = zombie.GetComponent<MiniEnemyAIController>();
                miniEnemy.player=playerTransform;
                activeZombies.Add(zombie); // Track the spawned zombie
            }
        }
    }

    private Vector3 GetRandomOffset()
    {
        float offsetX = Random.Range(-1f, 1f); // Random offset in X direction
        float offsetZ = Random.Range(-1f, 1f); // Random offset in Z direction
        return new Vector3(offsetX, 0, offsetZ); // No Y offset to keep on the same plane
    }


    private List<MazeCell> GetMazeCellsWithinRadius(Vector3 center, float radius)
    {
        float minRadius = 5f;
        List<MazeCell> validCells = new List<MazeCell>();

        foreach (MazeCell cell in _mazeGrid)
        {
            if (cell == null || cell.IsRoom) // Skip null or non-walkable cells
                continue;

            float distance = Vector3.Distance(center, cell.transform.position);
            if (distance <= radius && distance > minRadius)
            {
                validCells.Add(cell);
            }
        }

        return validCells;
    }

    void Start()
{
    _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];

    if (terrain == null)
    {
        Debug.LogError("No Terrain component found on this GameObject. Please attach this script to a Terrain.");
        return;
    }

    TerrainData terrainData = terrain.terrainData;

    // Get terrain dimensions
    float terrainWidth = terrainData.size.x;
    float terrainDepth = terrainData.size.z;
    Vector3 terrainPosition = terrain.GetPosition(); // World position of the terrain

    // Calculate the size of each maze cell based on the terrain and grid dimensions
    cellSizeX = terrainWidth / _mazeWidth;
    cellSizeZ = terrainDepth / _mazeDepth;

    // Generate maze grid
    for (int x = 0; x < _mazeWidth; x++)
    {
        for (int z = 0; z < _mazeDepth; z++)
        {
            // Calculate cell position relative to terrain
            float cellX = terrainPosition.x + (x + 0.5f) * cellSizeX;
            float cellZ = terrainPosition.z + (z + 0.5f) * cellSizeZ;
            float cellY = terrain.SampleHeight(new Vector3(cellX, 0, cellZ)) + terrainPosition.y; // Adjust for terrain height

            Vector3 cellPosition = new Vector3(cellX, cellY, cellZ);

            // Instantiate the maze cell
            MazeCell cell = Instantiate(_mazeCellPrefab, cellPosition, Quaternion.identity, floorParent.transform);
            // Set grid indices for this cell
            cell.GridX = x;
            cell.GridZ = z;

            
            cell.transform.localScale = new Vector3(cellSizeX, cell.transform.localScale.y, cellSizeZ);
            _mazeGrid[x, z] = cell;

            
        }
    }

    GenerateRooms();

    // Connect rooms to the maze
    ConnectRooms();

    // Start generating the maze
    GenerateMaze(null, _mazeGrid[0, 0]);

    ReplaceWallsInMaze();

    SpawnPlayerAndEnemy();

    BakeNavMesh();
    
}

private void BakeNavMesh()
    {
        if (navMeshSurface != null)
        {
            Debug.Log("Baking NavMesh...");
            navMeshSurface.BuildNavMesh(); // Bake the NavMesh
        }
        else
        {
            Debug.LogError("NavMeshSurface is not assigned!");
        }
    }

private void SpawnPlayerAndEnemy()
{
    // Spawn Player in a random cell that is not a room cell
    MazeCell randomCell;
    do
    {
        randomCell = _mazeGrid[Random.Range(0, _mazeWidth), Random.Range(0, _mazeDepth)];
    } 
    while (randomCell.IsRoom); // Ensure the chosen cell is not a room cell

    // Offset the player position to the right
    Vector3 offset = new Vector3(2.0f, 0, 0); // Adjust the X offset as needed
    Vector3 playerPosition = randomCell.transform.position + offset;

    if (playerTransform != null)
    {
        Debug.Log($"Setting Player position to: {playerPosition}");
        if (playerTransform.TryGetComponent<Rigidbody>(out Rigidbody playerRb))
        {
            playerRb.MovePosition(playerPosition);
        }
        else
        {
            playerTransform.position = playerPosition;
        }
    }
    else
    {
        Debug.LogError("Player object is not assigned in the Inspector!");
    }

    // Spawn Enemy in the middle of the first room
    if (_rooms.Count > 0)
    {
        Room firstRoom = _rooms[0];
        Vector3 roomCenter = firstRoom.position;
        roomCenter.y=2f;
        if (enemy != null)
        {
            Debug.Log($"Setting Enemy position to: {roomCenter}");
            if (enemy.TryGetComponent<Rigidbody>(out Rigidbody enemyRb))
            {
                enemyRb.MovePosition(roomCenter);
            }
            else
            {
                enemy.position = roomCenter;
            }
        }
        else
        {
            Debug.LogError("Enemy object is not assigned in the Inspector!");
        }
    }
    else
    {
        Debug.LogWarning("No rooms available to spawn the enemy.");
    }
}

    public MazeCell[,] GetMazeGrid()
    {
        return _mazeGrid;
    }

    public int GetMazeWidth()
    {
        return _mazeWidth;
    }

 
private void ReplaceWallsInMaze()
{
    bool isFlagActive = true;

    foreach (MazeCell cell in _mazeGrid)
    {
        if (cell == null) continue;

        // Get the dictionary of walls with their archway status
        var walls = cell.GetWalls();

        foreach (var wallEntry in walls)
        {
            string wallName = wallEntry.Key; // e.g., "leftWall", "frontWall"
            GameObject wallObject = wallEntry.Value.wallObject; // The wall GameObject
            bool isArchway = wallEntry.Value.isArchway; // Archway status

            if (wallObject == null) continue;

            // Get the position and scale of the wall
            Vector3 wallPosition = wallObject.transform.position;
            Vector3 scale = wallObject.transform.localScale;

            // Adjust position to align with terrain
            float terrainHeight = terrain.SampleHeight(wallPosition) + terrain.GetPosition().y;
            wallPosition.y = terrainHeight;

            // Determine rotation and wall orientation
            Quaternion rotation = Quaternion.identity;
            bool isVerticalWall = wallName == "leftWall" || wallName == "rightWall";

            if (isVerticalWall)
            {
                rotation = Quaternion.Euler(0, 90, 0); // Rotate 90 degrees for vertical walls
            }

            // Destroy the old wall
            Destroy(wallObject);

            // Fixed starting position for the first segment
            float segmentLength = 5.9f; // Length of one segment
            int fullWalls = Mathf.FloorToInt(cellSizeX / _maze_configuration);
            float remainder = (cellSizeX % _maze_configuration) / _maze_configuration;

            Vector3 currentPosition = wallPosition;

            if (isArchway)
            {
                if (!cell.IsRoom)
                {
                    // Calculate total length of the wall segments
                    float archwayTotalLength = (fullWalls + remainder) * segmentLength;

                    // Calculate the starting position offset
                    float archwayOffset = (archwayTotalLength / 2) - (segmentLength / 2);

                    // Calculate the starting position for the first segment
                    Vector3 archwayStartPosition = wallPosition;
                    if (isVerticalWall)
                    {
                        archwayStartPosition.z -= archwayOffset; // Adjust for vertical walls
                    }
                    else
                    {
                        archwayStartPosition.x -= archwayOffset; // Adjust for horizontal walls
                    }

                    currentPosition = archwayStartPosition;

                    GameObject archway;
                    if (cell.IsRoomEntrance)
                    {
                        // Place an archway with door prefab
                        archway = Instantiate(_archwayWithDoorPrefab, currentPosition, rotation, floorParent.transform);
                        allObjects.Add(archway);
                    }
                    else
                    {
                        // Place an archway prefab
                        archway = Instantiate(_archwayPrefab, currentPosition, rotation, floorParent.transform);
                        allObjects.Add(archway);
                    }

                    archway.transform.localScale = scale; // Adjust the scale of the archway if needed

                    fullWalls -= 1;

                    // Update the position for the next wall
                    if (isVerticalWall)
                    {
                        currentPosition.z += segmentLength; // Move along Z-axis for vertical walls
                    }
                    else
                    {
                        currentPosition.x += segmentLength; // Move along X-axis for horizontal walls
                    }

                    while (fullWalls > 0)
                    {
                        GameObject newWall = Instantiate(_plainWallPrefab, currentPosition, rotation, floorParent.transform);
                        allObjects.Add(newWall);
                        newWall.transform.localScale = scale; // Use the original scale for each wall

                        // Update the position for the next wall
                        if (isVerticalWall)
                        {
                            currentPosition.z += segmentLength; // Move along Z-axis for vertical walls
                        }
                        else
                        {
                            currentPosition.x += segmentLength; // Move along X-axis for horizontal walls
                        }

                        fullWalls--;
                    }

                    if (remainder > 0)
                    {
                        // Calculate the scale for the remainder wall
                        float scaledLength = segmentLength * remainder;

                        // Adjust the position to center the scaled wall correctly
                        Vector3 adjustedPosition = currentPosition;
                        if (isVerticalWall)
                        {
                            adjustedPosition.z -= (segmentLength - scaledLength) / 2; // Center along Z-axis
                        }
                        else
                        {
                            adjustedPosition.x -= (segmentLength - scaledLength) / 2; // Center along X-axis
                        }

                        // Place the remainder wall
                        GameObject remainderWall = Instantiate(_plainWallPrefab, adjustedPosition, rotation, floorParent.transform);
                        remainderWall.transform.localScale = new Vector3(remainder, scale.y, scale.z);
                        allObjects.Add(remainderWall);
                    }
                }
                continue;
            }

            // Calculate total length of the wall segments
            float totalLength = (fullWalls + remainder) * segmentLength;

            // Calculate the starting position offset
            float offset = (totalLength / 2) - (segmentLength / 2);

            // Calculate the starting position for the first segment
            Vector3 startPosition = wallPosition;
            if (isVerticalWall)
            {
                startPosition.z -= offset; // Adjust for vertical walls
            }
            else
            {
                startPosition.x -= offset; // Adjust for horizontal walls
            }

            currentPosition = startPosition;

            while (fullWalls > 0)
            {
                
                GameObject newWall = Instantiate(_wall.wallPrefab, currentPosition, rotation, floorParent.transform);
                allObjects.Add(newWall);
                newWall.transform.localScale = scale; // Use the original scale for each wall

                // Update the position for the next wall
                if (isVerticalWall)
                {
                    currentPosition.z += segmentLength; // Move along Z-axis for vertical walls
                }
                else
                {
                    currentPosition.x += segmentLength; // Move along X-axis for horizontal walls
                }

                fullWalls--;

                _wall.toggleWall(isFlagActive);
                isFlagActive = !isFlagActive;
            }

            if (remainder > 0)
            {
            
                // Calculate the scale for the remainder wall
                float scaledLength = segmentLength * remainder;

                // Adjust the position to center the scaled wall correctly
                Vector3 adjustedPosition = currentPosition;
                if (isVerticalWall)
                {
                    adjustedPosition.z -= (segmentLength - scaledLength) / 2; // Center along Z-axis
                }
                else
                {
                    adjustedPosition.x -= (segmentLength - scaledLength) / 2; // Center along X-axis
                }

                // Place the remainder wall
                GameObject remainderWall = Instantiate(_plainWallPrefab, adjustedPosition, rotation, floorParent.transform);
                remainderWall.transform.localScale = new Vector3(remainder, scale.y, scale.z);
                allObjects.Add(remainderWall);
            }
        }
    }
}

    public int GetMazeDepth()
    {
        return _mazeDepth;
    }

    private void ConnectRooms()
{
    foreach (Room room in _rooms)
    {
        // Store valid front edge cells of the room
        List<Vector2Int> frontEdgeCells = new List<Vector2Int>();

        // Determine the z position of the front of the room (using the prefab instance position)
        float roomFrontZ = room.position.z;

        // Collect valid front edge cells
        for (int dx = 0; dx < room.Width; dx++)
        {
            int x = room.X + dx;
            int z = room.Z + room.Depth; // Only consider cells beyond the room's depth
            if (z >= 0 && z < _mazeDepth && x >= 0 && x < _mazeWidth)
            {
                MazeCell candidateCell = _mazeGrid[x, z];
                if (candidateCell != null && candidateCell.transform.position.z > roomFrontZ)
                {
                    frontEdgeCells.Add(new Vector2Int(x, z));
                }
            }
        }

        // Ensure we have valid front edge cells
        if (frontEdgeCells.Count == 0)
        {
            Debug.LogWarning($"No valid front edge cells found for room at ({room.X}, {room.Z})");
            continue;
        }

        // Randomly pick one front edge cell
        Vector2Int entrancePos = frontEdgeCells[Random.Range(0, frontEdgeCells.Count)];

        // Connect the room to the maze
        MazeCell roomEdgeCell = _mazeGrid[entrancePos.x, entrancePos.y];
        MazeCell roomCell = _mazeGrid[Mathf.Clamp(entrancePos.x, room.X, room.X + room.Width - 1),
                                      Mathf.Clamp(entrancePos.y, room.Z, room.Z + room.Depth - 1)];

        ClearWalls(roomEdgeCell, roomCell);
        // Mark the maze cell (roomEdgeCell) as a room opening
        roomEdgeCell.MarkAsRoomEntrance();
    }
}


    private void GenerateRooms()
    {
        int attempts = 0; // Track attempts to avoid infinite loops
        int maxAttempts = _roomCount * 10; // A safe limit for retries

        while (_rooms.Count < _roomCount && attempts < maxAttempts)
        {
            // Randomly determine room size
            int roomWidth = Random.Range(_minRoomSize, _maxRoomSize + 1);
            int roomDepth = Random.Range(_minRoomSize, _maxRoomSize + 1);

            // Randomly determine room position
            int x = Random.Range(1, _mazeWidth - roomWidth - 1);
            int z = Random.Range(1, _mazeDepth - roomDepth - 1);

            Room newRoom = new Room(x, z, roomWidth, roomDepth);

            // Check for overlaps
            // Check for overlaps
            if (!DoesRoomOverlap(newRoom))
            {
                _rooms.Add(newRoom);

                // Get the position of the top-left cell (or any reference cell for the room)
                MazeCell referenceCell = _mazeGrid[x, z];
                Vector3 basePosition = referenceCell.transform.position;

                // Store a reference to the instance in the room object
                newRoom.position = basePosition;

                // Mark cells as rooms and clear their walls
                for (int dx = 0; dx < roomWidth; dx++)
                {
                    for (int dz = 0; dz < roomDepth; dz++)
                    {
                        MazeCell cell = _mazeGrid[x + dx, z + dz];
                        cell.MarkAsRoom();
                        cell.Visit(); // Prevent maze generation here
                    }
                }
            }

            attempts++;
        }

        if (_rooms.Count < _roomCount)
        {
            Debug.LogWarning($"Only {_rooms.Count} rooms were placed out of {_roomCount} due to space constraints.");
        }
    }


    private bool DoesRoomOverlap(Room room)
{
    foreach (Room existingRoom in _rooms)
    {
        // Add the buffer to the overlap check
        if (room.X < existingRoom.X + existingRoom.Width + _roomBufferSize &&
            room.X + room.Width + _roomBufferSize > existingRoom.X &&
            room.Z < existingRoom.Z + existingRoom.Depth + _roomBufferSize &&
            room.Z + room.Depth + _roomBufferSize > existingRoom.Z)
        {
            return true; // Overlap or too close
        }
    }
    return false; // No overlap and buffer is maintained
}



    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        if (!(currentCell.IsVisited && currentCell.IsRoom)){
            currentCell.Visit();
            ClearWalls(previousCell, currentCell);
            MazeCell nextCell;

            do
            {
                nextCell = GetNextUnvisitedCell(currentCell);

                if (nextCell != null)
                {
                    GenerateMaze(currentCell, nextCell);
                }
            } while (nextCell != null);
        }
    }


    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        var unvisitedCells = GetUnvisitedCells(currentCell).ToList();
        

        if (unvisitedCells.Count == 0)
            return null;

        // Shuffle the list of unvisited cells for random selection
        int randomIndex = Random.Range(0, unvisitedCells.Count);
        return unvisitedCells[randomIndex];
    }

    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        int x = currentCell.GridX; // Use the stored grid X index
        int z = currentCell.GridZ; // Use the stored grid Z index

        if (x + 1 < _mazeWidth)
        {
            var cellToRight = _mazeGrid[x + 1, z];
            
            if (cellToRight.IsVisited == false)
            {
                yield return cellToRight;
            }
        }

        if (x - 1 >= 0)
        {
            var cellToLeft = _mazeGrid[x - 1, z];

            if (cellToLeft.IsVisited == false)
            {
                yield return cellToLeft;
            }
        }

        if (z + 1 < _mazeDepth)
        {
            var cellToFront = _mazeGrid[x, z + 1];

            if (cellToFront.IsVisited == false)
            {
                yield return cellToFront;
            }
        }

        if (z - 1 >= 0)
        {
            var cellToBack = _mazeGrid[x, z - 1];

            if (cellToBack.IsVisited == false)
            {
                yield return cellToBack;
            }
        }
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
        {
            return;
        }

        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }

        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }

        if (previousCell.transform.position.z < currentCell.transform.position.z)
        {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }

        if (previousCell.transform.position.z > currentCell.transform.position.z)
        {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }

}

public class Room
{
    public int X; // Top-left corner X
    public int Z; // Top-left corner Z
    public int Width;
    public int Depth;
    public Vector3 position;

    public Room(int x, int z, int width, int depth)
    {
        X = x;
        Z = z;
        Width = width;
        Depth = depth;
    }
}

