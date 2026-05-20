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
        SceneManager.LoadScene("freeplay");
    }

    public void GoToScanLevel1()
    {
        SceneManager.LoadScene("Scan_Story1");
    }

    public void GoToScanLevel2()
    {
        SceneManager.LoadScene("Scan_Story2");
    }

    public void GoToScanLevel3()
    {
        SceneManager.LoadScene("Scan_Story3");
    }

    public void GoToScanLevel4()
    {
        SceneManager.LoadScene("Scan_Story4");
    }

    public void GoToLevel()
    {
        SceneManager.LoadScene("Level");
    }

    public void ResetGame()
    {
        
        PlayerPrefs.DeleteAll();
        
        
        
        PlayerPrefs.Save();
        
        Debug.Log("Semua PlayerPrefs berhasil dihapus secara permanen!");

        
        
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}