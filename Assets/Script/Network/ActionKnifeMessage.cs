using System;

[Serializable]
public class ActionKnifeMessage : Message
{
    public string type = "action-knife";
    public string roomId;
}

[Serializable]
public class ActionKnifeMessageAnswer
{
}