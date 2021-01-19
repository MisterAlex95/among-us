using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Socket : MonoBehaviour
{
    public static Socket instance;
    public Player currentPlayer;
    public List<Room> rooms;

    public GameEvent startGameEvent;
    public GameEvent deathEvent;
    public GameEvent ownDeathEvent;
    public GameEvent disconnectEvent;

    // Network
    public UdpClient client;
    public IPAddress serverIp;
    public string hostIp;
    public int hostPort;
    public IPEndPoint hostEndPoint;
    private object asyncLock;
    private List<GameEvent> eventQueue;

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

        client = new UdpClient();
        client.Connect(hostEndPoint);
        client.Client.Blocking = false;

        currentPlayer = new Player();
        rooms = new List<Room>();
        eventQueue = new List<GameEvent>();
        asyncLock = new object();

        HandCheck();
        client.BeginReceive(new AsyncCallback(processDgram), client);
        StartCoroutine(PingCoroutine());
    }

    public void HandCheck()
    {
        HandCheckMessage d = new HandCheckMessage();
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());
    }
    public void CreateRoom(int imposters, int maxPlayers, bool isPrivate)
    {
        CreateRoomMessage d = new CreateRoomMessage();
        d.uuid = currentPlayer.uuid;
        d.imposters = imposters;
        d.maxPlayers = maxPlayers;
        d.isPrivate = isPrivate;
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());
    }
    public void JoinRoom(string roomId)
    {
        JoinRoomMessage d = new JoinRoomMessage();
        d.roomId = roomId;
        d.uuid = currentPlayer.uuid;
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());
    }
    public void ListRoom()
    {
        ListRoomMessage d = new ListRoomMessage();
        d.uuid = currentPlayer.uuid;
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());
    }
    public void Ping()
    {
        PingMessage d = new PingMessage();
        d.uuid = currentPlayer.uuid;
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());
    }
    public void SwitchPrivacity(bool isPrivate)
    {
        SwitchPrivacityMessage d = new SwitchPrivacityMessage();
        d.uuid = currentPlayer.uuid;
        d.roomId = currentPlayer.room.id;
        d.isPrivate = isPrivate;
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());
    }
    public void LaunchGame()
    {
        LaunchGameMessage d = new LaunchGameMessage();
        d.uuid = currentPlayer.uuid;
        d.roomId = currentPlayer.room.id;
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());
    }
    public void ActionKnife()
    {
        ActionKnifeMessage d = new ActionKnifeMessage();
        d.roomId = currentPlayer.room.id;
        d.uuid = currentPlayer.uuid;
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());
    }

    void Update()
    {
        if (eventQueue.Count == 0) return;

        lock (asyncLock)
        {
            foreach (GameEvent ge in eventQueue)
            {
                ge.Raise();
            }
            eventQueue.Clear();
        }
    }

    IEnumerator PingCoroutine()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(4f);
            Ping();
        }
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
                        currentPlayer.uuid = JsonUtility.FromJson<HandCheckAnswer>(data[1]).uuid;
                        break;
                    }
                case "create-room":
                    {
                        currentPlayer.room.id = JsonUtility.FromJson<CreateRoomAnswer>(data[1]).id;
                        currentPlayer.room.maxPlayers = JsonUtility.FromJson<CreateRoomAnswer>(data[1]).maxPlayers;
                        currentPlayer.room.imposters = JsonUtility.FromJson<CreateRoomAnswer>(data[1]).imposters;
                        currentPlayer.room.admin = JsonUtility.FromJson<CreateRoomAnswer>(data[1]).admin;
                        currentPlayer.room.isPrivate = JsonUtility.FromJson<CreateRoomAnswer>(data[1]).isPrivate;
                        JoinRoom(currentPlayer.room.id);
                        break;
                    }
                case "join-room":
                    {
                        currentPlayer.room.id = JsonUtility.FromJson<JoinRoomAnswer>(data[1]).id;
                        currentPlayer.color = JsonUtility.FromJson<JoinRoomAnswer>(data[1]).color;
                        currentPlayer.room.maxPlayers = JsonUtility.FromJson<JoinRoomAnswer>(data[1]).maxPlayers;
                        currentPlayer.room.imposters = JsonUtility.FromJson<JoinRoomAnswer>(data[1]).imposters;
                        currentPlayer.room.admin = JsonUtility.FromJson<JoinRoomAnswer>(data[1]).admin;
                        currentPlayer.room.isPrivate = JsonUtility.FromJson<JoinRoomAnswer>(data[1]).isPrivate;
                        break;
                    }
                case "list-room":
                    {
                        Room[] receivedRooms = JsonUtility.FromJson<ListRoomMessageAnswer>(data[1]).rooms;
                        rooms.Clear();
                        foreach (Room roomInfo in receivedRooms.ToList())
                        {
                            rooms.Add(new Room(roomInfo.id, roomInfo.maxPlayers, roomInfo.imposters, roomInfo.nbrPlayer, roomInfo.isPrivate));
                        }
                        break;
                    }
                case "connexion":
                    {
                        // Spawn new player
                        Player newPlayer = new Player();
                        newPlayer.uuid = JsonUtility.FromJson<ConnexionMessageAnswer>(data[1]).uuid;
                        newPlayer.color = JsonUtility.FromJson<ConnexionMessageAnswer>(data[1]).color;
                        Vector3 position = JsonUtility.FromJson<ConnexionMessageAnswer>(data[1]).position;
                        newPlayer.position = position;
                        PlayerManager.instance.NewConnexion(newPlayer);
                        break;
                    }
                case "disconnect":
                    {
                        // Remove the player
                        string uuid = JsonUtility.FromJson<DisconnectMessageAnswer>(data[1]).uuid;
                        lock (asyncLock)
                        {
                            eventQueue.Add(disconnectEvent);
                            PlayerManager.instance.Disconnect(uuid);
                        }
                        break;
                    }
                case "launch-game":
                    {
                        currentPlayer.imposter = JsonUtility.FromJson<LaunchGameMessageAnswer>(data[1]).imposter;
                        lock (asyncLock)
                        {
                            eventQueue.Add(startGameEvent);
                            PlayerManager.instance.isRunning = true;
                        }
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
                case "killed":
                    {
                        lock (asyncLock)
                        {
                            eventQueue.Add(ownDeathEvent);
                            currentPlayer.isDead = true;
                        }
                        break;
                    }
                case "death":
                    {
                        string killedId = JsonUtility.FromJson<DeathMessageAnswer>(data[1]).killedId;
                        lock (asyncLock)
                        {
                            eventQueue.Add(deathEvent);
                            PlayerManager.instance.GetPlayer(killedId).isDead = true;
                        }
                        break;
                    }
                case "error":
                    {
                        // todo :)
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