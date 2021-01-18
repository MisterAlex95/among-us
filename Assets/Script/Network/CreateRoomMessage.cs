using System;

[Serializable]
public class CreateRoomMessage : Message
{
    public string type = "create-room";
    public int maxPlayers;
    public int imposters;
    public bool isPrivate;
}

[Serializable]
public class CreateRoomAnswer
{
    public int maxPlayers;
    public int imposters;
    public string id;
    public string admin;
    public bool isPrivate;
}