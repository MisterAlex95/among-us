using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class MenuManager : MonoBehaviour
{

    public GameObject MainMenu;
    public GameObject OnlineMenu;
    public GameObject LocalMenu;
    public GameObject HowToPlayMenu;
    public GameObject FreeplayMenu;

    // Create Room
    public GameObject CreateMenu;
    public Slider impostersSlider;
    public Slider maxPlayersSlider;

    // List Room
    public GameObject ListMenu;
    public GameObject Content;
    public GameObject RoomInfo;

    // Join Room
    public InputField inputCode;

    private GameObject currentMenu;

    void Start()
    {
        currentMenu = MainMenu;
    }

    public void GoToOnlineMenu()
    {
        currentMenu.SetActive(false);
        currentMenu = OnlineMenu;
        currentMenu.SetActive(true);
    }

    public void GoToMainMenu()
    {
        currentMenu.SetActive(false);
        currentMenu = MainMenu;
        currentMenu.SetActive(true);
    }

    public void GoToCreateMenu()
    {
        currentMenu.SetActive(false);
        currentMenu = CreateMenu;
        currentMenu.SetActive(true);
    }

    public void GoToFindMenu()
    {
        Socket.instance.ListRoom();
        currentMenu.SetActive(false);
        currentMenu = ListMenu;
        currentMenu.SetActive(true);
    }

    public void JoinButton()
    {
        if (inputCode.text != null)
        {
            Socket.instance.JoinRoom(inputCode.text.ToUpper());
            SceneManager.LoadScene("Lobby");
        }
    }

    public void RefreshListButton()
    {
        List<Room> rooms = Socket.instance.rooms;

        foreach (Room room in rooms)
        {
            if (GameObject.Find(room.id) == null)
            {
                GameObject roomInfoGO = Instantiate(RoomInfo, Content.transform) as GameObject;
                roomInfoGO.name = room.id;
                Text textRoomId = roomInfoGO.GetComponentsInChildren<Text>().ToList().Find(x => x.name == "RoomId");
                textRoomId.text = room.id;
                Text textPlayer = roomInfoGO.GetComponentsInChildren<Text>().ToList().Find(x => x.name == "PlayerCount");
                textPlayer.text = room.nbrPlayer + "/" + room.maxPlayers;
                Text textImposter = roomInfoGO.GetComponentsInChildren<Text>().ToList().Find(x => x.name == "ImpostersCount");
                textImposter.text = room.imposters.ToString();

            }
        }
        Socket.instance.ListRoom();
    }


    public void CancelToOnlineMenu()
    {
        currentMenu.SetActive(false);
        currentMenu = OnlineMenu;
        currentMenu.SetActive(true);
    }

    public void ConfirmRoomCreation()
    {
        Socket.instance.CreateRoom((int)impostersSlider.value, (int)maxPlayersSlider.value);
        SceneManager.LoadScene("Lobby");
    }
}
