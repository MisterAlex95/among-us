using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject playerPrefab;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("There is more than one instance of GameManager !");
            return;
        }
        instance = this;
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
}
