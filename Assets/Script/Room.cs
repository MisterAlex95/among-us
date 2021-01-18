using System;

[Serializable]
public class Room
{
    public string id;
    public string admin;
    public int maxPlayers;
    public int nbrPlayer;
    public int imposters;
    public bool isPrivate;

    public Room() { }
    public Room(string _uuid, int _maxPlayers, int _imposters, string _admin, bool _isPrivate = false)
    {
        id = _uuid;
        imposters = _imposters;
        maxPlayers = _maxPlayers;
        admin = _admin;
        isPrivate = _isPrivate;
    }

    public Room(string _uuid, int _maxPlayers, int _imposters, int _nbrPlayer, bool _isPrivate = false)
    {
        id = _uuid;
        imposters = _imposters;
        maxPlayers = _maxPlayers;
        nbrPlayer = _nbrPlayer;
        isPrivate = _isPrivate;
    }
}