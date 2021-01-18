using System;

[Serializable]
public class LaunchGameMessage : Message
{
    public string type = "launch-game";
    public string roomId;
}

[Serializable]
public class LaunchGameMessageAnswer
{
    public bool imposter;
}