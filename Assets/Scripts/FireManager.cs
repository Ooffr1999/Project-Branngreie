using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LevelGenerator))]
public class FireManager : MonoBehaviour
{
    public float fireSpreadTime;
    public GameObject Fire_Prefab;
    public LayerMask whatIsFire;
    [HideInInspector]
    public List<GameObject> firePool;
    GameObject[,] stageFire;

    int fireActivatedAmount = 0;
    int width, depth;
    LevelGenerator _levelGenerator;

    private void Awake()
    {
        _levelGenerator = GetComponent<LevelGenerator>();
    }

    public void InitPool(int poolSize)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject fire = Instantiate(Fire_Prefab, Fire_Prefab.transform.position, Fire_Prefab.transform.rotation);
            firePool.Add(fire);
            firePool[firePool.Count - 1].SetActive(false);
            firePool[firePool.Count - 1].transform.parent = this.transform;
            firePool[firePool.Count - 1].transform.localScale *= _levelGenerator.sizeModifier;
        }
    }

    public void InitFire(Vector2Int initFireAmountRange, int mapWidth, int mapDepth)
    {
        ClearFire();

        stageFire = new GameObject[mapWidth, mapDepth];
        int index = 0;

        //Place Fire
        for (int y = 0; y < mapDepth; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                stageFire[x, y] = firePool[index];
                stageFire[x, y].transform.position = new Vector3((-(mapWidth / 2) + x) * _levelGenerator.sizeModifier, stageFire[x, y].transform.position.y, y * _levelGenerator.sizeModifier);

                index++;
            }
        }

        int rand = Random.Range(initFireAmountRange.x, initFireAmountRange.y);

        for (int i = 0; i < rand; i++)
        {
            int x = Random.Range(0, mapWidth);
            int y = Random.Range(0, mapDepth);

            stageFire[x, y].SetActive(true);
        }

        width = mapWidth;
        depth = mapDepth;

        StopCoroutine(spreadFire());
        StartCoroutine(spreadFire());
    }

    public Vector2Int[] FireSpreadDirections(int x, int y)
    {
        Vector2Int[] directions = new Vector2Int[4];

        directions[0] = new Vector2Int(x + 1, y);
        directions[1] = new Vector2Int(x, y - 1);
        directions[2] = new Vector2Int(x - 1, y);
        directions[3] = new Vector2Int(x, y + 1);

        return directions;
    }

    public void ClearFire()
    {
        fireActivatedAmount = 0;

        for (int y = 0; y < depth; y++)
        {
            for(int x = 0; x < width; x++)
            {
                stageFire[x, y].SetActive(false);
            }
        }
    }

    IEnumerator spreadFire()
    {
        while (true)
        {
            yield return new WaitForSeconds(fireSpreadTime);

            AudioManager._instance.playListSound("Ignite");

            List<GameObject> stageFireList = new List<GameObject>();

            for (int y = 0; y < depth; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (stageFire[x, y].activeSelf == true)
                    {
                        if (x + 1 < width)
                            stageFireList.Add(stageFire[x + 1, y]);

                        if (y + 1 < depth)
                            stageFireList.Add(stageFire[x, y + 1]);

                        if (x - 1 >= 0)
                            stageFireList.Add(stageFire[x - 1, y]);

                        if (y - 1 >= 0)
                            stageFireList.Add(stageFire[x, y - 1]);
                    }
                }
            }

            for (int i = 0; i < stageFireList.Count; i++)
            {
                stageFireList[i].SetActive(true);
            }
        }
    }
}