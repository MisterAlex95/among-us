using System;

[Serializable]
public class ListRoomMessage : Message
{
    public string type = "list-room";
}

[Serializable]
public class ListRoomMessageAnswer
{
    public Room[] rooms;
}
