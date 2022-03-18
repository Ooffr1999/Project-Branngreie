using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room_Ref : MonoBehaviour
{
    public enum RoomType { hallway, livingRoom}
    public RoomType roomType;

    public Transform[] exits;
    
    public Transform[] upExits;
    public Transform[] downExits;
    public Transform[] leftExits;
    public Transform[] rightExits;

    public int northEntranceLength()
    {
        return upExits.Length;
    }

    public int southEntranceLength()
    {
        return downExits.Length;
    }

    public int westEntranceLength()
    {
        return leftExits.Length;
    }

    public int eastEntranceLength()
    {
        return rightExits.Length;
    }
}
