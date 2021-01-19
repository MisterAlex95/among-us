using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform spawnPoint;
    public Sprite deadBody;
    public GameObject imposterText;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("There is more than one instance of GameManager !");
            return;
        }
        instance = this;
    }

    void Start()
    {
        GameObject playerGO = GameObject.Find("Player");
        playerGO.transform.position = spawnPoint.position;

        foreach (Player player in PlayerManager.instance.players)
        {
            player.position = spawnPoint.position;
        }

        imposterText.SetActive(Socket.instance.currentPlayer.imposter);
    }

    void Update()
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

        // We update the players
        foreach (Player player in PlayerManager.instance.players)
        {
            if (player.instantiate)
            {
                GameObject playerGO = GameObject.Find(player.uuid);
                if (playerGO != null && !player.isDead)
                {
                    playerGO.transform.position = player.position; // Apply Player position to object
                }
            }
        }
    }

    public void onDeath()
    {
        // For now we have to check each player as we cannot send value with event
        foreach (Player player in PlayerManager.instance.players)
        {
            if (player.isDead)
            {
                GameObject playerGO = GameObject.Find(player.uuid);
                if (playerGO)
                {
                    playerGO.GetComponent<SpriteRenderer>().sprite = deadBody;
                }

            }
        }
    }
}
