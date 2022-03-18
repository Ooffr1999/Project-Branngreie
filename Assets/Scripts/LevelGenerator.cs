using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public Vector2Int roomWidthRange;
    public Vector2Int roomDepthRange;
    public int roomHeight;

    public RoomComponents[] roomTypes;

    [HideInInspector]
    public List<GameObject> floorPool = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> wallPool = new List<GameObject>();

    private void Start()
    {
        int floorTilePoolAmount = roomWidthRange.y * roomDepthRange.y;
        int wallTilePoolAmount = (roomWidthRange.y * roomHeight) + ((roomDepthRange.y * roomHeight) * 2);

        InitPool(floorTilePoolAmount, wallTilePoolAmount);
        GenRoom();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            CleanRoomBuild();
            GenRoom();
        }
    }

    void InitPool(int floorPoolSize, int wallPoolSize)
    {
        //Generate floor tiles
        for (int i = 0; i < floorPoolSize; i++)
        {
            floorPool.Add(Instantiate(roomTypes[0].floorPieces[0], transform.position, roomTypes[0].floorPieces[0].transform.rotation));
            floorPool[floorPool.Count - 1].SetActive(false);
            floorPool[floorPool.Count - 1].transform.parent = this.transform;
        }

        //Generate wall tiles
        for (int i = 0; i < wallPoolSize; i++)
        {
            wallPool.Add(Instantiate(roomTypes[0].wallPieces[0], transform.position, roomTypes[0].wallPieces[0].transform.rotation));
            wallPool[wallPool.Count - 1].SetActive(false);
            wallPool[wallPool.Count - 1].transform.parent = this.transform;
        }
    }

    public void GenRoom()
    {
        int widthLength = genRoomSize(roomWidthRange.x, roomWidthRange.y);
        int depthLength = genRoomSize(roomDepthRange.x, roomDepthRange.y);

        Vector3 startPos = genStartPosition(widthLength, depthLength);

        ///Place floor objects
        int i = 0;
        for (int d = 0; d < depthLength; d++)
        {
            for (int w = 0; w < widthLength; w++)
            {
                Vector3 spawnPos = new Vector3(startPos.x + w, 0, startPos.z + d);
                floorPool[i].SetActive(true);
                floorPool[i].transform.position = spawnPos;

                i++;  
            }
        }

        ///Place wall objects
        i = 0;
        for (int h = 0; h < roomHeight; h++)
        {
            for (int b = 0; b < widthLength; b++)
            {
                Vector3 spawnPos = new Vector3(startPos.x + b, 0.5f + h, startPos.z + (depthLength - 1) + 0.5f);
                wallPool[i].SetActive(true);
                wallPool[i].transform.position = spawnPos;

                i++;
            }

            for (int b = 0; b < depthLength; b++)
            {
                Vector3 leftSpawnPos = new Vector3(startPos.x - 0.5f, 0.5f + h, startPos.z + b);
                Vector3 leftSpawnRotation = new Vector3(0, -90, 0);

                wallPool[i].SetActive(true);

                wallPool[i].transform.position = leftSpawnPos;
                wallPool[i].transform.eulerAngles = leftSpawnRotation;

                i++;
            }

            for (int b = 0; b < depthLength; b++)
            {
                Vector3 rightSpawnPos = new Vector3(startPos.x + (widthLength - 1) + 0.5f, 0.5f + h, startPos.z + b);
                Vector3 rightSpawnRotation = new Vector3(0, 90, 0);

                wallPool[i].SetActive(true);

                wallPool[i].transform.position = rightSpawnPos;
                wallPool[i].transform.eulerAngles = rightSpawnRotation;

                i++;
            }
        }
    }

    void CleanRoomBuild()
    {
        for (int i = 0; i < floorPool.Count; i++)
        {
            floorPool[i].SetActive(false);
        }

        for (int i = 0; i < wallPool.Count; i++)
        {
            wallPool[i].SetActive(false);
            wallPool[i].transform.eulerAngles = Vector3.zero;
        }
    }

    public int genRoomSize(int minSize, int maxSize)
    {
        return Random.Range(minSize, maxSize);
    }

    public Vector3 genStartPosition(int widthSize, int depthSize)
    {
        return new Vector3(-Mathf.FloorToInt(widthSize / 2), 0, -Mathf.FloorToInt(depthSize / 2));
    }
}

[System.Serializable]
public struct RoomComponents
{
    public string name;
    public GameObject[] floorPieces;
    public GameObject[] wallPieces;
}