using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PathGenerator))]
public class LevelGenerator : MonoBehaviour
{
    public int roomsTraversalBeforeExitAmount;

    public float sizeModifier = 1;

    [SerializeField]
    GameObject floorTile, wallTile, door;

    [SerializeField]
    GameObject[] enemyTypes;

    [SerializeField]
    Light roomDirectionalLight;

    [SerializeField]
    Room[] rooms;
    [SerializeField]
    LayerMask _obstacle;

    int roomsTraversed;
    GameObject _player;
    CharacterController _playerController;
    
    //Lists for ? lagre object pools.
    [HideInInspector]
    public List<GameObject> floorPool = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> wallPool = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> doorPool = new List<GameObject>();
    public List<GameObject> blocksPool = new List<GameObject>();
    public List<GameObject> enemyPool = new List<GameObject>();

    int _roomType;
    [HideInInspector]
    public Vector3 _roomStart, _roomEnd;
    BoxCollider _roomBounds;
    [HideInInspector]
    public List<Vector3> grid;
    [HideInInspector]
    public PathGenerator _pathGenerator;
    [HideInInspector]
    public FireManager _fireManager;

    //Til n? er det greit ? generere poolet p? Start og generere rommet etterp? s? man har noe ? se p?
    private void Start()
    {
        _player = GameObject.Find("Player");
        _playerController = _player.GetComponent<CharacterController>();

        _roomBounds = GetComponent<BoxCollider>();

        _pathGenerator = GetComponent<PathGenerator>();
        _fireManager = GetComponent<FireManager>();

        InitPool(200, 200, 2, 30, 200);
        GenRoom();
    }

    //Generer et Pool til gulv og et Pool til vegger
    void InitPool(int floorPoolSize, int wallPoolSize, int doorPoolSize, int blocksPerItemSize, int fireSize)
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
        foreach (Room r in rooms)
        {
            for (int b = 0; b < r.roomObjects.Length; b++)
            {
                for (int i = 0; i < blocksPerItemSize; i++)
                {
                    GameObject newObject = Instantiate(r.roomObjects[b], transform.position, transform.rotation);
                    newObject.transform.parent = this.transform;
                    blocksPool.Add(newObject);
                }
            }
        }

        for (int i = 0; i < enemyTypes.Length; i++)
        {
            for (int m = 0; m < 5; m++)
            {
                GameObject mon = Instantiate(enemyTypes[i], transform.position, transform.rotation);
                mon.SetActive(false);
                enemyPool.Add(mon);
            }
        }

        //Generate fire for the fireManager
        _fireManager.InitPool(fireSize);
    }

    //Generer selve rommet
    public void GenRoom()
    {
        Room room = getRoom();

        int mapWidth = genRoomSize(room.roomWidthRange.x, room.roomWidthRange.y);
        int mapDepth = genRoomSize(room.roomDepthRange.x, room.roomDepthRange.y);

        //Plasser rommet midt i scenen, og f? et utgangspunkt for hvor objekter genereres
        Vector3 startPos = genStartPosition(mapWidth, mapDepth);

        //Lag et grid pathfinding og objektplassering kan g? ut ifra
        GenerateGrid(mapWidth, mapDepth, startPos, sizeModifier);
        _pathGenerator.InitPathFinding(grid, _obstacle, sizeModifier);
        
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
            //Generer h?yreveggen i rommet
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
        #endregion

        GenInterior(room);                                                                                                  //Generate an interior to the room

        #region Fill room with monsters
        int randMon = Random.Range(0, room.maxMonsterAmount + 1);
        Debug.Log("Spawned " + randMon + " monsters");

        for (int m = 0; m < randMon; m++)
        {
            enemyPool[m].SetActive(true);
            enemyPool[m].transform.position = _pathGenerator.getRandomAccesiblePosition(Vector3.zero, Mathf.Infinity);     
        }
        #endregion

        //Generate fire
        _fireManager.InitFire(room.fireAmountRange, mapWidth, mapDepth);
    }

    public void GenInterior(Room room)
    {
        //Place whiteblocks around the room
        CleanRoomInterior();

        StartCoroutine(checkObjectPosition(room));

        StartCoroutine(getPathAfterRoomGenerate(room));
    }

    //Rens rommet f?r neste GenRoom
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

    public void ClearMonsters()
    {
        //Deactivate enemies
        for (int i = 0; i < enemyPool.Count; i++)
        {
            enemyPool[i].SetActive(false);
        }
    }

    public void getRoomTraversal()
    {
        if (Vector3.Distance(_player.transform.position, doorPool[1].transform.position) <=
            Vector3.Distance(_player.transform.position, doorPool[0].transform.position))
            roomsTraversed++;   
    }

    //En funksjon for ? sette spilleren p? plass
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

    //F? et tall mellom to variabler. Basically Random.range, men ser litt finere ut etter min mening
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
        _roomType = rand;
        return rooms[rand];
    }

    public Vector3 getGridSquareFromPosition(Vector3 position)
    {
        return MapGridGenerator.getGridSquareFromPosition(grid, position);
    }

    public Vector3 getRandomRoomSquare()
    {
        int rand = Random.Range(0, grid.Count);

        if (grid[rand] == getGridSquareFromPosition(doorPool[0].transform.position))
            rand++;
        if (grid[rand] == getGridSquareFromPosition(doorPool[1].transform.position))
            rand++;

        return getGridSquareFromPosition(grid[rand]);
    }

    IEnumerator getPathAfterRoomGenerate(Room room)
    {
        yield return new WaitForFixedUpdate();
        
        if (_pathGenerator.CheckPath(_roomStart, _roomEnd) == false)
        {   
            GenInterior(room);
        }
    }

    IEnumerator checkObjectPosition(Room room)
    {
        List<GameObject> blockList = new List<GameObject>();

        int roomBlockStartValue = 0;

        
        for (int i = 0; i < _roomType; i++)
        {
            roomBlockStartValue += rooms[i].roomObjects.Length;
        }

        roomBlockStartValue *= 30;

        Debug.Log("Roomblockvalue is " + roomBlockStartValue);

        for (int i = 0; i < grid.Count; i++)
        {
            int percentage = Random.Range(0, 101);

            if (percentage < room.roomObjectSpawnChancePerTile && grid[i] != _roomStart && grid[i] != _roomEnd)
            {
                int rand = Random.Range(roomBlockStartValue, roomBlockStartValue + (rooms[_roomType].roomObjects.Length * 30));

                blocksPool[rand].SetActive(true);
                blocksPool[rand].transform.position = grid[i];
                blockList.Add(blocksPool[rand]);
            }
        }

        yield return new WaitForFixedUpdate();

        for (int y = 0; y < blockList.Count; y++)
        {
            if (y + 1 < blockList.Count)
            {
                BoxCollider col1 = blockList[y].GetComponent<BoxCollider>();
                BoxCollider col2 = blockList[y + 1].GetComponent<BoxCollider>();

                if (col1.bounds.Intersects(col2.bounds))
                    blockList[y].SetActive(false);
            }
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

    [Space(10)]
    public int maxMonsterAmount;

    [Space(10)]
    public Vector2Int fireAmountRange;
}