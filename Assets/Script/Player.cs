using System;
using UnityEngine;

[Serializable]
public class Player
{
    public string uuid;
    public string color;
    public Room room;
    public Vector3 position;
    public bool instantiate;

    public Player()
    {
        room = new Room();
    }
    public Player(string _uuid, string _color)
    {
        room = new Room();
        uuid = _uuid;
        color = _color;
        position = new Vector3(0, 0, 0);
        instantiate = false;
    }
}