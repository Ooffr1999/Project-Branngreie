using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathGenerator))]
public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    float sizeModifier = 1;

    [SerializeField]
    GameObject floorTile, wallTile, door, block;

    [SerializeField]
    Room[] rooms;

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
 
    //Til nå er det greit å generere poolet på Start og generere rommet etterpå så man har noe å se på
    private void Start()
    {
        _player = GameObject.Find("Player");
        _playerController = _player.GetComponent<CharacterController>();

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

        Vector3 startPos = genStartPosition(mapWidth, mapDepth);

        PathGenerator._instance.GenerateGrid(mapWidth, mapDepth, startPos, sizeModifier);
        
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
                floorPool[i].GetComponent<MeshRenderer>().material = room.floorMats[0];

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

                i++;
            }
            //Generer venstreveggen i rommet
            for (int b = 0; b < mapWidth; b++)
            {
                Vector3 leftSpawnPos = new Vector3(startPos.x - 0.5f, 0.5f + h, startPos.z + b) * sizeModifier;
                Vector3 leftSpawnRotation = new Vector3(0, -90, 0);

                wallPool[i].SetActive(true);

                wallPool[i].transform.position = leftSpawnPos;
                wallPool[i].transform.eulerAngles = leftSpawnRotation;
                wallPool[i].transform.localScale *= sizeModifier;
                wallPool[i].GetComponent<MeshRenderer>().material = room.wallMats[0];

                i++;
            }
            //Generer høyreveggen i rommet
            for (int b = 0; b < mapWidth; b++)
            {
                Vector3 rightSpawnPos = new Vector3(startPos.x + mapWidth - 0.5f, 0.5f + h, startPos.z + b) * sizeModifier;
                Vector3 rightSpawnRotation = new Vector3(0, 90, 0);

                wallPool[i].SetActive(true);

                wallPool[i].transform.position = rightSpawnPos;
                wallPool[i].transform.eulerAngles = rightSpawnRotation;
                wallPool[i].transform.localScale *= sizeModifier;
                wallPool[i].GetComponent<MeshRenderer>().material = room.wallMats[0];

                i++;
            }
        }

        ///Place doors
        for (int d = 0; d < 2; d++)
        {
            int percentage = Random.Range(0, 100);
            int doorDepthPosition = Random.Range(0, mapDepth);

            doorPool[d].SetActive(true);

            Vector3 doorPosition = new Vector3();
            doorPosition.z = doorDepthPosition * sizeModifier;
            doorPosition.y = 0.5f;

            switch (d)
            {
                case 0:
                    doorPosition.x = startPos.x - 0.5f;
                    break;
                case 1:
                    doorPosition.x = startPos.x + (mapWidth - 0.5f);
                    break;
            }

            doorPosition.x *= sizeModifier;

            doorPool[d].transform.position = doorPosition;
        }

        PathGenerator._instance.GetStartAndEndPositions(doorPool[0].transform.position, doorPool[1].transform.position);

        setPlayerPosition(PathGenerator._instance.getStartPos());
        
        Music currentTempTrack = MusicManager._instance.GetPlayingTempTrack();

        if (currentTempTrack != null)
        {
            if (currentTempTrack.TrackName != room.tempTrackToPlay)
                MusicManager._instance.FadeBetweenTracks(currentTempTrack.TrackName, room.tempTrackToPlay, 0.5f);
        }
        else MusicManager._instance.PlayTrack(room.tempTrackToPlay);

        GenInterior();
    }

    public void GenInterior()
    {
        //Place whiteblocks around the room
        List<Vector3> grid = PathGenerator._instance.getGridPoints();

        CleanRoomInterior();

        for (int i = 0; i < grid.Count; i++)
        {
            int percentage = Random.Range(0, 100);

            if (percentage < 45 && grid[i] != PathGenerator._instance.getStartPos() && grid[i] != PathGenerator._instance.getEndPos())
            {
                blocksPool[i].SetActive(true);
                blocksPool[i].transform.position = grid[i];
            }
        }

        Debug.Log("Start pathfinding");

        StartCoroutine(getPathAfterRoomGenerate());
    }

    //Rens rommet før neste GenRoom
    public void CleanRoomBuild()
    {
        for (int i = 0; i < floorPool.Count; i++)
        {
            floorPool[i].SetActive(false);
            floorPool[i].transform.localScale = Vector3.one;
        }

        for (int i = 0; i < wallPool.Count; i++)
        {
            wallPool[i].SetActive(false);
            wallPool[i].transform.eulerAngles = Vector3.zero;
            wallPool[i].transform.localScale = Vector3.one;
        }

        for (int i = 0; i < doorPool.Count; i++)
        {
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

    //Generer en tilfeldig romtype og return den
    Room getRoom()
    {
        int rand = Random.Range(0, rooms.Length);
        return rooms[rand];
    }

    IEnumerator getPathAfterRoomGenerate()
    {
        yield return new WaitForSeconds(0.02f);

        if (PathGenerator._instance.GetPath() == false)
        {
            //iterations++;
            GenInterior();
            //Debug.Log("Iterations = " + iterations);
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
}