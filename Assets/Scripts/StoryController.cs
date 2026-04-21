using UnityEngine;
using UnityEngine.UI;

public class StoryController : MonoBehaviour
{
    [Header("Daftar Popup Story")]
    public GameObject[] popups; 
    
    private int currentIndex = 0;

    void Start()
    {
        // Langsung muncul popup pertama saat scene loading
        OpenStory();
    }

    // Fungsi baru untuk dipanggil tombol "Cerita"
    public void OpenStory()
    {
        currentIndex = 0; // Reset ke cerita pertama
        ShowPopup(currentIndex);
    }

    public void PrevStory()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowPopup(currentIndex);
        }
        else
        {
            CloseAllPopups();
        }
    }

    public void NextStory()
    {
        if (currentIndex < popups.Length - 1)
        {
            currentIndex++;
            ShowPopup(currentIndex);
        }
        else
        {
            CloseAllPopups();
        }
    }

    public void CloseAllPopups()
    {
        foreach (GameObject p in popups)
        {
            p.SetActive(false);
        }
    }

    private void ShowPopup(int index)
    {
        CloseAllPopups();
        if (index >= 0 && index < popups.Length)
        {
            popups[index].SetActive(true);
        }
    }
}