using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartAR()
    {
        SceneManager.LoadScene("mackys");
    }

    public void OpenGuide()
    {
        Debug.Log("Guide button clicked");
    }
}