using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button[] levelButtons; // Masukkan Button L1, L2, L3, L4 ke sini sesuai urutan
    public float lockedOpacity = 0.5f; // Opacity untuk level yang terkunci

    void Start()
    {
        // --- TAMBAHKAN DI SINI ---
        // Jika game baru pertama kali dijalankan (belum ada data tersimpan),
        // anggap level 1 sudah selesai sehingga level 2 otomatis terbuka.
        if (!PlayerPrefs.HasKey("LastClearedLevel")) 
        {
            PlayerPrefs.SetInt("LastClearedLevel", 2);
            PlayerPrefs.Save(); // Simpan perubahan ke memori perangkat
        }
        // -------------------------

        RefreshLevelStatus();
    }

    public void RefreshLevelStatus()
    {
        // Mengambil data level mana yang boleh diakses
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
        btn.interactable = false; // Tidak bisa diklik
        SetButtonOpacity(btn, lockedOpacity);
    }

    void UnlockLevel(Button btn)
    {
        btn.interactable = true; // Bisa diklik
        SetButtonOpacity(btn, 1.0f); // Opacity penuh
    }

    void SetButtonOpacity(Button btn, float alpha)
    {
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        
        cg.alpha = alpha;
    }

    // Panggil fungsi ini saat menekan tombol "Lanjut" di popup finish level 2
    public void CompleteLevel(int levelIndex)
    {
        int currentReached = PlayerPrefs.GetInt("LastClearedLevel", 1);
        
        // Simpan hanya jika level yang diselesaikan lebih tinggi dari rekor sebelumnya
        if (levelIndex >= currentReached)
        {
            PlayerPrefs.SetInt("LastClearedLevel", levelIndex + 1);
            PlayerPrefs.Save();
        }
    }
}