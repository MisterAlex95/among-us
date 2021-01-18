using System;

[Serializable]
public class SwitchPrivacityMessage : Message
{
    public string type = "switch-privacity";
    public bool isPrivate;
    public string roomId;
}