using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class Socket : MonoBehaviour
{
    public static Socket instance;
    public UdpClient client;
    public IPAddress serverIp;
    public string hostIp;
    public int hostPort;
    public IPEndPoint hostEndPoint;
    public Transform playerTransform;
    private Vector3 nextPosition;
    public Player currentPlayer;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("There is more than one instance of Socket !");
            return;
        }
        instance = this;
    }

    void Start()
    {
        serverIp = IPAddress.Parse(hostIp);
        hostEndPoint = new IPEndPoint(serverIp, hostPort);

        nextPosition = new Vector3(0, 0, 0);

        client = new UdpClient();
        client.Connect(hostEndPoint);
        client.Client.Blocking = false;

        currentPlayer = new Player();

        // HandCheck to get a uuid
        HandCheckMessage d = new HandCheckMessage();
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());

        client.BeginReceive(new AsyncCallback(processDgram), client);
    }

    public void SendDgram(string evento, string msg)
    {
        byte[] dgram = Encoding.UTF8.GetBytes(evento + "_" + msg);
        Debug.Log("Send new :" + msg);
        client.Send(dgram, dgram.Length);
    }

    public void processDgram(IAsyncResult res)
    {
        try
        {
            byte[] recieved = client.EndReceive(res, ref hostEndPoint);
            string[] data = Encoding.UTF8.GetString(recieved).Split('_');
            Debug.Log("Receive - type :" + data[0] + " -data :" + data[1]);

            switch (data[0])
            {
                case "handcheck":
                    {
                        // Set current player
                        currentPlayer.uuid = JsonUtility.FromJson<HandCheckAnswer>(data[1]).uuid; // Assign uuid to the client
                        currentPlayer.color = JsonUtility.FromJson<HandCheckAnswer>(data[1]).color; // Assign color to the client
                        break;
                    }
                case "connexion":
                    {
                        // Spawn new player
                        Player newPlayer = new Player();
                        newPlayer.uuid = JsonUtility.FromJson<HandCheckAnswer>(data[1]).uuid; // Assign uuid to the client
                        newPlayer.color = JsonUtility.FromJson<HandCheckAnswer>(data[1]).color; // Assign color to the client
                        PlayerManager.instance.NewConnexion(newPlayer);
                        break;
                    }
                case "position":
                    {
                        Player playerData = JsonUtility.FromJson<Player>(data[1]);
                        Player player = PlayerManager.instance.GetPlayer(playerData.uuid);
                        if (player != null)
                        {
                            player.position.x = playerData.position.x;
                            player.position.y = playerData.position.y;
                            player.position.z = playerData.position.z;
                        }
                        break;
                    }
                default:
                    break;
            }

        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            throw ex;
        }

        // Receive next Message
        client.BeginReceive(new AsyncCallback(processDgram), client);
    }
}