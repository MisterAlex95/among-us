using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    public Button PrivateButton;
    public Text PrivateButtonText;
    public Text CodeText;
    public Text CounterText;

    private bool IsPrivate;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("There is more than one instance of LobbyManager !");
            return;
        }
        instance = this;
    }

    void Start()
    {
        IsPrivate = Socket.instance.currentPlayer.room.isPrivate;
    }

    void Update()
    {
        int count = PlayerManager.instance.players.Count + 1; // add +1 for you
        int totalCount = Socket.instance.currentPlayer.room.maxPlayers;
        CounterText.text = count + "/" + totalCount;

        if (Socket.instance.currentPlayer.room.admin == Socket.instance.currentPlayer.uuid)
        {
            PrivateButtonText.text = (IsPrivate) ? "Private" : "Public";
            string code = Socket.instance.currentPlayer.room.id;
            CodeText.text = "Code: " + code;
        }
        else
        {
            CodeText.gameObject.SetActive(false);
            PrivateButtonText.transform.parent.gameObject.SetActive(false);
        }
    }

    public void SwitchPrivacity()
    {
        IsPrivate = !IsPrivate;
        Socket.instance.SwitchPrivacity(IsPrivate);
    }
}
