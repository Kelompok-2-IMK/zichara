using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Level Buttons (urutan: L1, L2, L3, L4)")]
    public Button[] levelButtons;
    public float lockedOpacity = 0.5f;

    private readonly string[] sceneNames = {
        "Scan_Story1",
        "Scan_Story2",
        "Scan_Story3",
        "Scan_Story4"
    };

    void Start()
    {
        if (!PlayerPrefs.HasKey("LastClearedLevel"))
        {
            PlayerPrefs.SetInt("LastClearedLevel", 1);
            PlayerPrefs.Save();
        }

        RefreshLevelStatus();
    }

    public void RefreshLevelStatus()
    {
        int lastCleared = PlayerPrefs.GetInt("LastClearedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            if (levelNumber <= lastCleared)
                UnlockLevel(levelButtons[i]);
            else
                LockLevel(levelButtons[i]);
        }
    }

    void LockLevel(Button btn)
    {
        btn.interactable = false;
        SetOpacity(btn, lockedOpacity);
    }

    void UnlockLevel(Button btn)
    {
        btn.interactable = true;
        SetOpacity(btn, 1.0f);
    }

    void SetOpacity(Button btn, float alpha)
    {
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = alpha;
    }

    // Dipanggil dari onClick tiap tombol level
    public void LoadLevel(int levelToLoad)
    {
        if (levelToLoad < 1 || levelToLoad > sceneNames.Length)
        {
            Debug.LogError($"LoadLevel: level {levelToLoad} tidak valid!");
            return;
        }

        Debug.Log($"Memuat Level {levelToLoad}: {sceneNames[levelToLoad - 1]}");
        SceneManager.LoadScene(sceneNames[levelToLoad - 1]);
    }
}