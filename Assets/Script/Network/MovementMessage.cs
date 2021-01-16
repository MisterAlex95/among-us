using UnityEngine;
using System;

[Serializable]
public class MovementMessage
{
    public int id;
    public string uuid;
    public string roomId;
    public string type = "movement";
    public Vector2 position;
}