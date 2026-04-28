using UnityEngine;

public class StoryController : MonoBehaviour
{
    [Header("UI Status Per Misi")]
    public GameObject status1; // Taruh object 'status1' dari Hierarchy ke sini
    public GameObject status2; // Taruh object 'status2' dari Hierarchy ke sini

    [Header("Daftar Popup Story")]
    public GameObject[] popups; 
    
    private int currentIndex = 0;
    private int currentMissionStartBound = 0;
    private int currentMissionEndBound = 1; // Default misi 1 (index 0-1)

    void Start()
    {
        // Awal game, set ke misi 1
        SetupMissionUI(1);
    }

    // Fungsi untuk ganti set popup & status bar
    public void SetupMissionUI(int missionNumber)
    {
        CloseAllPopups();
        
        if (missionNumber == 1)
        {
            status1.SetActive(true);
            status2.SetActive(false);
            currentMissionStartBound = 0; // popup 2.1.1
            currentMissionEndBound = 1;   // popup 2.1.2
        }
        else if (missionNumber == 2)
        {
            status1.SetActive(false);
            status2.SetActive(true);
            currentMissionStartBound = 2; // popup 2.2.1
            currentMissionEndBound = 3;   // popup 2.2.2
        }
        else if (missionNumber == 3) 
        {
            status1.SetActive(false);
            status2.SetActive(false);
            // Misal popupStory2.finish ada di index ke-4 dalam array popups
            currentMissionStartBound = 4; 
            currentMissionEndBound = 4;   
        }
        currentIndex = currentMissionStartBound;
        ShowPopup(currentIndex);
    }

    public void NextStory()
    {
        // Batasi supaya Next tidak kebablasan ke misi selanjutnya sebelum waktunya
        if (currentIndex < currentMissionEndBound)
        {
            currentIndex++;
            ShowPopup(currentIndex);
        }
        else
        {
            CloseAllPopups();
        }
    }

    public void PrevStory()
    {
        // Batasi supaya tidak balik ke popup misi sebelumnya
        if (currentIndex > currentMissionStartBound)
        {
            currentIndex--;
            ShowPopup(currentIndex);
        }
    }

    // Fungsi-fungsi lain (CloseAllPopups, ShowPopup) tetap sama seperti sebelumnya
    public void CloseAllPopups() { foreach (GameObject p in popups) p.SetActive(false); }
    private void ShowPopup(int index) { CloseAllPopups(); popups[index].SetActive(true); }
    
    // Fungsi untuk tombol "Cerita" agar munculin popup misi yang sekarang saja
    public void OpenStory() { currentIndex = currentMissionStartBound; ShowPopup(currentIndex); }
}