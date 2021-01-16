using System;

[Serializable]
public class HandCheckMessage
{
    public string type = "handcheck";
}

[Serializable]
public class HandCheckAnswer
{
    public string uuid;
    public string color;
}