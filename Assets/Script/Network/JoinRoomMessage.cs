using System;

[Serializable]
public class JoinRoomMessage
{
    public string type = "join-room";
    public string roomId;
    public string uuid;
}

[Serializable]
public class JoinRoomAnswer
{
    public string id;
    public string color;
}