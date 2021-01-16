using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

public class Socket : MonoBehaviour
{
    public static Socket instance;
    public Player currentPlayer;
    public List<Room> rooms;

    // Network
    public UdpClient client;
    public IPAddress serverIp;
    public string hostIp;
    public int hostPort;
    public IPEndPoint hostEndPoint;

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

        HandCheck();
        client.BeginReceive(new AsyncCallback(processDgram), client);
    }

    public void HandCheck()
    {
        HandCheckMessage d = new HandCheckMessage();
        SendDgram("JSON", JsonUtility.ToJson(d).ToString());
    }
    public void CreateRoom(int imposters, int maxPlayers)
    {
        CreateRoomMessage d = new CreateRoomMessage();
        d.uuid = currentPlayer.uuid;
        d.imposters = imposters;
        d.maxPlayers = maxPlayers;
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
                        Room newRoom = new Room();
                        newRoom.id = JsonUtility.FromJson<CreateRoomAnswer>(data[1]).id;
                        newRoom.maxPlayers = JsonUtility.FromJson<CreateRoomAnswer>(data[1]).maxPlayers;
                        newRoom.imposters = JsonUtility.FromJson<CreateRoomAnswer>(data[1]).imposters;
                        newRoom.admin = JsonUtility.FromJson<CreateRoomAnswer>(data[1]).admin;
                        currentPlayer.room = newRoom;
                        JoinRoom(newRoom.id);
                        break;
                    }
                case "join-room":
                    {
                        currentPlayer.room.id = JsonUtility.FromJson<JoinRoomAnswer>(data[1]).id;
                        currentPlayer.color = JsonUtility.FromJson<JoinRoomAnswer>(data[1]).color;
                        break;
                    }
                case "list-room":
                    {
                        Room[] receivedRooms = JsonUtility.FromJson<ListRoomMessageAnswer>(data[1]).rooms;
                        rooms.Clear();
                        foreach (Room roomInfo in receivedRooms.ToList())
                        {
                            rooms.Add(new Room(roomInfo.id, roomInfo.maxPlayers, roomInfo.imposters, roomInfo.nbrPlayer));
                        }
                        break;
                    }
                case "connexion":
                    {
                        // Spawn new player
                        Player newPlayer = new Player();
                        newPlayer.uuid = JsonUtility.FromJson<ConnexionMessageAnswer>(data[1]).uuid;
                        newPlayer.color = JsonUtility.FromJson<ConnexionMessageAnswer>(data[1]).color;
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