using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void GoToModeSelection()
    {
        SceneManager.LoadScene("modeSelection");
    }

    public void GoToLegends()
    {
        SceneManager.LoadScene("Legends");
    }

    public void BackToMain()
    {
        SceneManager.LoadScene("main");
    }

    public void StartFreeplay()
    {
        SceneManager.LoadScene("mackys");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Keluar");
    }
}