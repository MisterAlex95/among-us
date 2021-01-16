using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public GameObject MainMenu;
    public GameObject OnlineMenu;
    public GameObject CreateMenu;
    public GameObject LocalMenu;
    public GameObject HowToPlayMenu;
    public GameObject FreeplayMenu;

    public Slider impostersSlider;
    public Slider maxPlayersSlider;

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

    public void CancelToOnlineMenu()
    {
        currentMenu.SetActive(false);
        currentMenu = OnlineMenu;
        currentMenu.SetActive(true);
    }

    public void ConfirmRoomCreation()
    {
        Socket.instance.CreateRoom((int)impostersSlider.value, (int)maxPlayersSlider.value);
        SceneManager.LoadScene("Game");
    }
}
