using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    public Button PrivateButton;
    public Text PrivateButtonText;
    public Text CodeText;
    public Text CounterText;

    private bool IsPrivate = true;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("There is more than one instance of LobbyManager !");
            return;
        }
        instance = this;
    }

    void Update()
    {
        int count = PlayerManager.instance.players.Count + 1; // add +1 for you
        int totalCount = Socket.instance.currentPlayer.room.maxPlayers;
        string code = Socket.instance.currentPlayer.room.id;
        CounterText.text = count + "/" + totalCount;
        CodeText.text = "Code: " + code;
    }

    public void SwitchPrivacity()
    {
        IsPrivate = !IsPrivate;
        PrivateButtonText.text = (IsPrivate) ? "Private" : "Public";
    }
}
