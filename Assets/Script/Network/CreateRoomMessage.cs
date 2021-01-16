using System;

[Serializable]
public class CreateRoomMessage
{
    public string type = "create-room";
    public string uuid;
    public int maxPlayers;
    public int imposters;
}

[Serializable]
public class CreateRoomAnswer
{
    public int maxPlayers;
    public int imposters;
    public string id;
    public string admin;
}