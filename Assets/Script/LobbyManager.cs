using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    public GameObject playerPrefab;
    public Button LaunchButton;
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
            PrivateButton.gameObject.SetActive(false);
            LaunchButton.gameObject.SetActive(false);
        }

        // We update the players
        foreach (Player player in PlayerManager.instance.players)
        {
            if (!player.instantiate)
            {
                GameObject newPlayerGO = Instantiate(playerPrefab, new Vector3(player.position.x, player.position.y, player.position.z), Quaternion.identity);
                Color color;
                if (ColorUtility.TryParseHtmlString("#" + player.color, out color))
                    newPlayerGO.GetComponent<SpriteRenderer>().color = color;
                newPlayerGO.name = player.uuid;
                player.instantiate = true;
            }
            else
            {
                GameObject playerGO = GameObject.Find(player.uuid);
                if (playerGO != null)
                {
                    playerGO.transform.position = player.position; // Apply Player position to object
                }
            }
        }
    }

    public void SwitchPrivacity()
    {
        IsPrivate = !IsPrivate;
        Socket.instance.SwitchPrivacity(IsPrivate);
    }
    public void RunGame()
    {
        foreach (Player player in PlayerManager.instance.players)
        {
            GameObject playerGO = GameObject.Find(player.uuid);
            DontDestroyOnLoadScene.instance.AddToDontDestroyOnLoad(playerGO);
        }
        GameObject currentPlayerGO = GameObject.Find("Player");
        DontDestroyOnLoadScene.instance.AddToDontDestroyOnLoad(currentPlayerGO);

        GameObject FieldOfViewGO = GameObject.Find("FieldOfView");
        DontDestroyOnLoadScene.instance.AddToDontDestroyOnLoad(FieldOfViewGO);

        SceneManager.LoadScene("Map_1");
    }
    public void LaunchGame()
    {
        Socket.instance.LaunchGame();
    }
    public void Disconnect()
    {
        // We destroy the disconnected players
        foreach (string playerToRemove in PlayerManager.instance.playerToRemove)
        {
            GameObject p = GameObject.Find(playerToRemove);
            if (p)
            {
                Destroy(p);
            }
        }

        // The list is cleared after the destroy
        PlayerManager.instance.playerToRemove.Clear();
    }
}
