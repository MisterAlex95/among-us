using System;
using UnityEngine;

[Serializable]
public class Room
{
    public string id;
    public string admin;
    public int maxPlayers;
    public int imposters;

    public Room() { }
    public Room(string _uuid, int _maxPlayers, int _imposters, string _admin)
    {
        id = _uuid;
        imposters = _imposters;
        maxPlayers = _maxPlayers;
        admin = _admin;
    }
}