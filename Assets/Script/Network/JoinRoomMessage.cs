using System;

[Serializable]
public class JoinRoomMessage : Message
{
    public string type = "join-room";
    public string roomId;
}

[Serializable]
public class JoinRoomAnswer
{
    public string id;
    public string color;
}