using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public List<GameObject> roomLayouts;

    [HideInInspector]
    public List<GameObject> currentRooms;

    GameObject currentRoom;
    GameObject previousRoom;

    enum DirectionsOut { up, down, left, right}
    DirectionsOut _directionsOut;

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            int rand = Random.Range(0, roomLayouts.Count);
            GameObject room = Instantiate(roomLayouts[rand], Vector3.zero, roomLayouts[rand].transform.rotation);
            
            currentRooms.Add(room);
            room.SetActive(false);           
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            GenRoom();
    }

    void GenRoom()
    {
        if (previousRoom != null)
        {
            previousRoom.SetActive(false);
            previousRoom = currentRoom;
            
            Room_Ref _Ref_PreviousRoom = previousRoom.GetComponent<Room_Ref>();

            currentRoom = currentRooms[getRand(0, currentRooms.Count)];
            currentRoom.SetActive(true);


            currentRoom.transform.position = _Ref_PreviousRoom.exits[getRand(0, _Ref_PreviousRoom.exits.Length)].position;
        }

        else 
        {
            int rand = Random.Range(0, currentRooms.Count);
            currentRoom = currentRooms[rand];
            currentRoom.SetActive(true);

            currentRoom.transform.position = Vector3.zero;
            previousRoom = currentRoom;
        }
    }

    int getRand(int min, int max)
    {
        return Random.Range(min, max);
    }

    DirectionsOut getDirection()
    {
        return (DirectionsOut)Random.Range(0, 4);
    }
}
