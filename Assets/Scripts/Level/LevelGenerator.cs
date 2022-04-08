using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PathGenerator))]
public class LevelGenerator : MonoBehaviour
{
    public LayerMask obstacleLayer;
    public int roomsTraversalBeforeExitAmount;

    [SerializeField]
    float sizeModifier = 1;

    [SerializeField]
    GameObject floorTile, wallTile, door, block;

    [SerializeField]
    Light roomDirectionalLight;

    [SerializeField]
    Room[] rooms;
    [SerializeField]
    LayerMask _obstacle;

    int roomsTraversed;
    GameObject _player;
    CharacterController _playerController;
    
    //Lists for å lagre object pools.
    [HideInInspector]
    public List<GameObject> floorPool = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> wallPool = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> doorPool = new List<GameObject>();
    List<GameObject> blocksPool = new List<GameObject>();

    [HideInInspector]
    public Vector3 _roomStart, _roomEnd;
    BoxCollider _roomBounds;
    List<Vector3> grid;
    [HideInInspector]
    public PathGenerator _pathGenerator;

    //Til nå er det greit å generere poolet på Start og generere rommet etterpå så man har noe å se på
    private void Start()
    {
        _player = GameObject.Find("Player");
        _playerController = _player.GetComponent<CharacterController>();

        _roomBounds = GetComponent<BoxCollider>();

        _pathGenerator = GetComponent<PathGenerator>();

        InitPool(200, 200, 10);
        GenRoom();
    }

    //Generer et Pool til gulv og et Pool til vegger
    void InitPool(int floorPoolSize, int wallPoolSize, int doorPoolSize)
    {
        //Generate floor tiles to the floorpool
        for (int i = 0; i < floorPoolSize; i++)
        {
            floorPool.Add(Instantiate(floorTile, transform.position, floorTile.transform.rotation));
            floorPool[floorPool.Count - 1].SetActive(false);
            floorPool[floorPool.Count - 1].transform.parent = this.transform;
        }

        //Generate wall tiles to the wallpool
        for (int i = 0; i < wallPoolSize; i++)
        {
            wallPool.Add(Instantiate(wallTile, transform.position, wallTile.transform.rotation));
            wallPool[wallPool.Count - 1].SetActive(false);
            wallPool[wallPool.Count - 1].transform.parent = this.transform;
        }

        //Generates doors to the rooms
        for (int i = 0; i < doorPoolSize; i++)
        {
            doorPool.Add(Instantiate(door, transform.position, door.transform.rotation));
            doorPool[doorPool.Count - 1].SetActive(false);
            doorPool[doorPool.Count - 1].transform.parent = this.transform;
        }

        //Generate blocks for room interior
        for (int i = 0; i < 200; i++)
        {
            blocksPool.Add(Instantiate(block, transform.position, block.transform.rotation));
            blocksPool[blocksPool.Count - 1].SetActive(false);
            blocksPool[blocksPool.Count - 1].transform.parent = this.transform;
        }
    }

    //Generer selve rommet
    public void GenRoom()
    {
        Room room = getRoom();

        int mapWidth = genRoomSize(room.roomWidthRange.x, room.roomWidthRange.y);
        int mapDepth = genRoomSize(room.roomDepthRange.x, room.roomDepthRange.y);

        //Plasser rommet midt i scenen, og få et utgangspunkt for hvor objekter genereres
        Vector3 startPos = genStartPosition(mapWidth, mapDepth);

        //Lag et grid pathfinding og objektplassering kan gå ut ifra
        GenerateGrid(mapWidth, mapDepth, startPos, sizeModifier);
        _pathGenerator.InitPathFinding(grid, obstacleLayer, sizeModifier);
        
        ///Place floor objects
        int i = 0;
        for (int d = 0; d < mapDepth; d++)
        {
            for (int w = 0; w < mapWidth; w++)
            {
                Vector3 spawnPos = new Vector3(startPos.x + w, 0, startPos.z + d) * sizeModifier;
                floorPool[i].SetActive(true);
                floorPool[i].transform.position = spawnPos;
                floorPool[i].transform.localScale *= sizeModifier;

                int rand = Random.Range(0, room.floorMats.Length);
                floorPool[i].GetComponent<MeshRenderer>().material = room.floorMats[rand];

                floorPool[i].isStatic = true;
                floorPool[i].GetComponent<NavMeshSurface>().BuildNavMesh();

                i++;
            }
        }

        ///Place wall objects
        i = 0;
        for (int h = 0; h < room.roomHeight; h++)
        {
            //Generer bakveggen i rommet
            for (int b = 0; b < mapWidth; b++)
            {
                Vector3 spawnPos = new Vector3(startPos.x + b, 0.5f + h, startPos.z + (mapDepth - 1) + 0.5f) * sizeModifier;
                wallPool[i].SetActive(true);
                wallPool[i].transform.position = spawnPos;
                wallPool[i].transform.localScale *= sizeModifier;
                wallPool[i].GetComponent<MeshRenderer>().material = room.wallMats[0];

                wallPool[i].isStatic = true;

                i++;
            }
            //Generer venstreveggen i rommet
            for (int b = 0; b < mapDepth; b++)
            {
                Vector3 leftSpawnPos = new Vector3(startPos.x - 0.5f, 0.5f + h, startPos.z + b) * sizeModifier;
                Vector3 leftSpawnRotation = new Vector3(0, -90, 0);

                wallPool[i].SetActive(true);

                wallPool[i].transform.position = leftSpawnPos;
                wallPool[i].transform.eulerAngles = leftSpawnRotation;
                wallPool[i].transform.localScale *= sizeModifier;

                int rand = Random.Range(0, room.wallMats.Length);
                wallPool[i].GetComponent<MeshRenderer>().material = room.wallMats[rand];

                wallPool[i].isStatic = true;

                i++;
            }
            //Generer høyreveggen i rommet
            for (int b = 0; b < mapDepth; b++)
            {
                Vector3 rightSpawnPos = new Vector3(startPos.x + mapWidth - 0.5f, 0.5f + h, startPos.z + b) * sizeModifier;
                Vector3 rightSpawnRotation = new Vector3(0, 90, 0);

                wallPool[i].SetActive(true);

                wallPool[i].transform.position = rightSpawnPos;
                wallPool[i].transform.eulerAngles = rightSpawnRotation;
                wallPool[i].transform.localScale *= sizeModifier;
                wallPool[i].GetComponent<MeshRenderer>().material = room.wallMats[0];

                wallPool[i].isStatic = true;

                i++;
            }
        }

        ///Place doors
        for (int d = 0; d < 2; d++)
        {
            int doorDepthPosition = Random.Range(0, mapDepth);

            doorPool[d].SetActive(true);

            Vector3 doorPosition = new Vector3();
            doorPosition.z = doorDepthPosition * sizeModifier;
            doorPosition.y = 0.5f;

            switch (d)
            {
                case 0:
                    if (_player.transform.position.x > 0)
                        doorPosition.x = startPos.x - 0.5f;
                    else doorPosition.x = startPos.x + (mapWidth - 0.5f);
                    break;
                case 1:
                    if (_player.transform.position.x > 0)
                        doorPosition.x = startPos.x + (mapWidth - 0.5f);
                    else doorPosition.x = startPos.x - 0.5f;

                    if (roomsTraversed >= roomsTraversalBeforeExitAmount)
                        if (Random.Range(0, 101) <= 30)
                        {
                            doorPool[d].GetComponent<MeshRenderer>().material.color = Color.green;
                            doorPool[d].tag = "Finish";
                        }
                        else
                        {
                            doorPool[d].GetComponent<MeshRenderer>().material.color = Color.white;
                            doorPool[d].tag = "Untagged";
                        }
                    break;
            }

            doorPosition.x *= sizeModifier;

            doorPool[d].transform.position = doorPosition;
            doorPool[d].isStatic = true;
        }

        //Retrieve start and end positions by getting the squares closest to the two doors
        _roomStart = getGridSquareFromPosition(doorPool[0].transform.position);
        _roomEnd = getGridSquareFromPosition(doorPool[1].transform.position);

        setPlayerPosition(_roomStart);                                                           //Set the player position to start
        
        Music currentTempTrack = MusicManager._instance.GetPlayingTempTrack();                                              //Start playing the temporary track of each room you're in
        if (currentTempTrack != null)
        {
            if (currentTempTrack.TrackName != room.tempTrackToPlay)
                MusicManager._instance.FadeBetweenTracks(currentTempTrack.TrackName, room.tempTrackToPlay, 0.5f);
        }                                                                                   
        else MusicManager._instance.PlayTrack(room.tempTrackToPlay);

        roomDirectionalLight.color = room.lightColor;                                                                       //Set light color in room

        #region Set Bounds of Room Trigger
        float boundsCenterX = 0;
        float boundsCenterZ = 0;

        if (mapWidth % 2 == 0)
            boundsCenterX = 0.5f * sizeModifier;
        if (mapDepth % 2 == 0)
            boundsCenterZ = 0.5f * sizeModifier;

        _roomBounds.center = new Vector3(-boundsCenterX, (room.roomHeight * sizeModifier) / 2, (mapDepth / 2) - boundsCenterZ);
        _roomBounds.size = new Vector3(mapWidth * sizeModifier, room.roomHeight * sizeModifier, mapDepth * sizeModifier);
        #endregion

        GenInterior(room);                                                                                                  //Generate an interior to the room
    }

    public void GenInterior(Room room)
    {
        //Place whiteblocks around the room
        CleanRoomInterior();

        for (int i = 0; i < grid.Count; i++)
        {
            int percentage = Random.Range(0, 101);

            if (percentage < room.roomObjectSpawnChancePerTile && grid[i] != _roomStart && grid[i] != _roomEnd)
            {
                //if (!PathGenerator._instance.evaluatePoint(PathGenerator._instance.getPoint(grid[i], 0)))
                    //return;

                blocksPool[i].SetActive(true);
                blocksPool[i].transform.position = grid[i];

                //Check if object fits within bounds
                BoxCollider col = blocksPool[i].GetComponent<BoxCollider>();

                //blocksPool[i].SetActive(getColliderBounds(col));

                /*
                if (Physics.CheckBox(col.center, col.size, transform.rotation, _obstacle))
                    blocksPool[i].SetActive(false);
                */
            }
        }

        //StartCoroutine(checkObjectPosition());

        StartCoroutine(getPathAfterRoomGenerate(room));
    }

    //Rens rommet før neste GenRoom
    public void CleanRoomBuild()
    {
        for (int i = 0; i < floorPool.Count; i++)
        {
            floorPool[i].isStatic = false;
            floorPool[i].SetActive(false);
            floorPool[i].transform.localScale = Vector3.one;
        }

        for (int i = 0; i < wallPool.Count; i++)
        {
            wallPool[i].isStatic = false;
            wallPool[i].SetActive(false);
            wallPool[i].transform.eulerAngles = Vector3.zero;
            wallPool[i].transform.localScale = Vector3.one;
        }

        for (int i = 0; i < doorPool.Count; i++)
        {
            doorPool[i].isStatic = false;
            doorPool[i].SetActive(false);
        }
    }

    public void CleanRoomInterior()
    {
        for (int i = 0; i < blocksPool.Count; i++)
        {
            blocksPool[i].SetActive(false);
        }
    }

    public void getRoomTraversal()
    {
        if (Vector3.Distance(_player.transform.position, doorPool[1].transform.position) <=
            Vector3.Distance(_player.transform.position, doorPool[0].transform.position))
            roomsTraversed++;   
    }

    //En funksjon for å sette spilleren på plass
    void setPlayerPosition(Vector3 newPosition)
    {
        _playerController.enabled = false;

        Vector3 adjustedPosition = newPosition;
        newPosition.y = 0.5f;

        _player.transform.position = adjustedPosition;

        _playerController.enabled = true;
    }

    //Normaliser posisjonen til rommet
    Vector3 genStartPosition(int widthSize, int depthSize)
    {
        return new Vector3(-(widthSize / 2), 0, 0);
    }

    //Få et tall mellom to variabler. Basically Random.range, men ser litt finere ut etter min mening
    int genRoomSize(int minSize, int maxSize)
    {
        return Random.Range(minSize, maxSize);
    }

    public void GenerateGrid(int mapWidth, int mapDepth, Vector3 startPosition, float sizeModifier)
    {
        grid = MapGridGenerator.GenerateMapGrid(mapWidth, mapDepth, startPosition, sizeModifier);
    }

    //Generer en tilfeldig romtype og return den
    Room getRoom()
    {
        int rand = Random.Range(0, rooms.Length);
        return rooms[rand];
    }

    public Vector3 getGridSquareFromPosition(Vector3 position)
    {
        return MapGridGenerator.getGridSquareFromPosition(grid, position);
    }

    bool getColliderBounds(BoxCollider col)
    {
        Vector3 current = col.transform.position;

        if (!_roomBounds.bounds.Contains(current))
            return false;

        current.x -= col.bounds.extents.x;

        if (!_roomBounds.bounds.Contains(current))
            return false;

        current.x += col.bounds.extents.x * 2;

        if (!_roomBounds.bounds.Contains(current))
            return false;

        current.x -= col.bounds.extents.x;
        current.z -= col.bounds.extents.z;

        if (!_roomBounds.bounds.Contains(current))
            return false;

        current.z += col.bounds.extents.z * 2;

        if (!_roomBounds.bounds.Contains(current))
            return false;

        return true;
    }

    IEnumerator getPathAfterRoomGenerate(Room room)
    {
        yield return new WaitForFixedUpdate();
        
        if (_pathGenerator.CheckPath(_roomStart, _roomEnd) == false)
        {   
            GenInterior(room);
        }
    }

    IEnumerator checkObjectPosition()
    {
        yield return new WaitForSeconds(0.02f);
        
        for (int i = 0; i < blocksPool.Count; i++)
        {
            BoxCollider col = blocksPool[i].GetComponent<BoxCollider>();

            if (Physics.CheckBox(col.center, col.size, transform.rotation, _obstacle))
                blocksPool[i].SetActive(false);
        }

    }
}

[System.Serializable]
public struct Room
{
    public string name;

    [Space(10)]
    public Vector2Int roomWidthRange;
    public Vector2Int roomDepthRange;

    [Space(10)]
    public int roomHeight;

    [Space(10)]
    public string tempTrackToPlay;

    [Space(10)]
    public Material[] floorMats;
    public Material[] wallMats;

    [Space(10)]
    [Range(0, 100)]
    public float roomObjectSpawnChancePerTile;
    public GameObject[] roomObjects;

    [Space(10)]
    public Color lightColor;
}