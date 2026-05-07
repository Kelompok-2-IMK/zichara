using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryController : MonoBehaviour
{
    [Header("UI Status Per Misi")]
    public GameObject status1;
    public GameObject status2;

    [Header("Daftar Popup Story (urut sesuai array)")]
    public GameObject[] popups;

    private int currentIndex = 0;
    private int currentMissionStartBound = 0;
    private int currentMissionEndBound = 0;

    private void Start()
    {
        CloseAllPopups();
    }

    public void SetupMissionUI(int missionNumber)
    {
        CloseAllPopups();

        string sceneName = SceneManager.GetActiveScene().name;
        bool isLevel3 = sceneName == "Scan_Story3";

        if (missionNumber == 1)
        {
            if (status1 != null) status1.SetActive(true);
            if (status2 != null) status2.SetActive(false);
            currentMissionStartBound = 0;
            currentMissionEndBound = 1;
        }
        else if (missionNumber == 2)
        {
            if (status1 != null) status1.SetActive(false);
            if (status2 != null) status2.SetActive(true);
            currentMissionStartBound = 2;
            currentMissionEndBound = 3;
        }
        else if (missionNumber == 99) // finish
        {
            if (status1 != null) status1.SetActive(false);
            if (status2 != null) status2.SetActive(false);

            // Level 3 hanya punya 3 popup (0,1,2), finish di index 2
            // Level lain punya 5 popup (0,1,2,3,4), finish di index 4
            int finishIndex = isLevel3 ? 2 : 4;
            currentMissionStartBound = finishIndex;
            currentMissionEndBound = finishIndex;
        }

        currentIndex = currentMissionStartBound;
        ShowPopup(currentIndex);
    }

    public void NextStory()
    {
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
        if (currentIndex > currentMissionStartBound)
        {
            currentIndex--;
            ShowPopup(currentIndex);
        }
    }

    public void CloseAllPopups()
    {
        if (popups == null) return;
        foreach (GameObject p in popups)
            if (p != null) p.SetActive(false);
    }

    private void ShowPopup(int index)
    {
        if (popups == null || popups.Length == 0)
        {
            Debug.LogError("Popups array kosong!");
            return;
        }
        if (index < 0 || index >= popups.Length)
        {
            Debug.LogError($"ShowPopup index {index} out of range! Max: {popups.Length - 1}");
            return;
        }
        CloseAllPopups();
        popups[index].SetActive(true);
    }

    public void OpenStory()
    {
        currentIndex = currentMissionStartBound;
        ShowPopup(currentIndex);
    }
}