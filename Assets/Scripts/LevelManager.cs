using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Tambahan wajib untuk pindah scene

public class LevelManager : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button[] levelButtons; // Masukkan Button L1, L2, L3, L4 ke sini sesuai urutan
    public float lockedOpacity = 0.5f; // Opacity untuk level yang terkunci

    void Start()
    {
        // FIX 1: Ubah ke 1. Pemain baru harus mulai dari Level 1 yang terbuka, sisanya terkunci.
        if (!PlayerPrefs.HasKey("LastClearedLevel")) 
        {
            PlayerPrefs.SetInt("LastClearedLevel", 1);
            PlayerPrefs.Save(); 
        }

        RefreshLevelStatus();
    }

    public void RefreshLevelStatus()
    {
        int lastClearedLevel = PlayerPrefs.GetInt("LastClearedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;

            if (levelNumber <= lastClearedLevel)
            {
                UnlockLevel(levelButtons[i]);
            }
            else
            {
                LockLevel(levelButtons[i]);
            }
        }
    }

    void LockLevel(Button btn)
    {
        btn.interactable = false; 
        SetButtonOpacity(btn, lockedOpacity);
    }

    void UnlockLevel(Button btn)
    {
        btn.interactable = true; 
        SetButtonOpacity(btn, 1.0f); 
    }

    void SetButtonOpacity(Button btn, float alpha)
    {
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        
        cg.alpha = alpha;
    }

    // Panggil fungsi ini saat menekan tombol "Lanjut" di popup cerita TERAKHIR di suatu level
    public void CompleteLevel(int levelIndex)
    {
        int currentReached = PlayerPrefs.GetInt("LastClearedLevel", 1);
        
        if (levelIndex >= currentReached)
        {
            PlayerPrefs.SetInt("LastClearedLevel", levelIndex + 1);
            PlayerPrefs.Save();
        }
    }

    // FIX 2: Fungsi baru yang akan dipanggil saat pemain MENGKLIK tombol Level 1, 2, atau 3
    public void LoadLevel(int levelToLoad)
    {
        Debug.Log("Memuat Level: " + levelToLoad);
        
        // Simpan niat pemain ingin main level berapa
        PlayerPrefs.SetInt("TargetLevelToPlay", levelToLoad);
        PlayerPrefs.Save();

        // Pindah ke scene AR utama kamu (Pastikan nama scenenya benar, misal "ScanScene")
        SceneManager.LoadScene("ScanScene"); 
    }
}