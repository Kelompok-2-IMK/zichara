using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void GoToModeSelection()
    {
        SceneManager.LoadScene("ModeSelection");
    }

    public void GoToLegends()
    {
        SceneManager.LoadScene("Legends");
    }

    public void GoToPanduan()
    {
        SceneManager.LoadScene("Panduan");
    }

    public void GoToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void StartFreeplay()
    {
        SceneManager.LoadScene("Dev");
    }

    public void GoToScan()
    {
        SceneManager.LoadScene("Scan");
    }

    public void GoToLevel()
    {
        SceneManager.LoadScene("Level");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Keluar");
    }
}