using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelGenerator))]
public class FireManager : MonoBehaviour
{
    public GameObject Fire_Prefab;
    [HideInInspector]
    public List<GameObject> firePool;

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
        }
    }

    public void InitFire()
    {
        int startFireAmount = Random.Range(0, 10);

        for (int i = 0; i < startFireAmount; i++)
        {
            Vector3 pos = _levelGenerator.getRandomRoomSquare();
            pos.y += _levelGenerator.sizeModifier / 2;

            firePool[i].transform.position = pos;
            firePool[i].transform.localScale = Vector3.one * _levelGenerator.sizeModifier;
            firePool[i].SetActive(true);
        }
    }

    void ClearFire()
    {
        for (int i = 0; i < firePool.Count; i++)
        {
            firePool[i].transform.position = Fire_Prefab.transform.position;
            firePool[i].transform.rotation = Fire_Prefab.transform.rotation;
            firePool[i].transform.localScale = Vector3.one * _levelGenerator.sizeModifier;
        }
    }
}
