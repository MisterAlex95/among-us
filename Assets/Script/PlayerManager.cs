using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public List<Player> players = new List<Player>();

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("There is more than one instance of PlayerManager !");
            return;
        }
        instance = this;
    }

    public void NewConnexion(string uuid, string colorHex)
    {
        players.Add(new Player(uuid, colorHex));
    }

    public void NewConnexion(Player player)
    {
        players.Add(new Player(player.uuid, player.color));
    }

    public Player GetPlayer(string uuid)
    {
        return players.Find(player => player.uuid == uuid);
    }
}
