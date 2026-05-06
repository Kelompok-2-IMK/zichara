using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    [Header("Koleksi Settings")]
    // Masukkan GameObject 'dian', 'hua', 'shou', 'ji' ke sini
    public Image[] allPhotos; 
    // Harus sama persis dengan ID yang kamu kirim dari Vuforia (dian, hua, dll)
    public string[] itemNames; 

    [Header("UI Pop Up Settings")]
    public GameObject[] allPopUpPanels; 

    void Start()
    {
        CloseAllPopUps();
        UpdateAllIcons();
    }

    // Ini fungsi utama buat ganti opacity
    public void UpdateAllIcons()
    {
        if (allPhotos == null || allPhotos.Length == 0) return;

        for (int i = 0; i < allPhotos.Length; i++)
        {
            if (allPhotos[i] != null)
            {
                // Cek apakah data tersimpan di PlayerPrefs
                bool isUnlocked = PlayerPrefs.GetInt(itemNames[i], 0) == 1;
                
                // Ambil warna asli Image-nya
                Color tempColor = allPhotos[i].color;

                if (isUnlocked)
                {
                    tempColor.a = 1f; // Opacity 100% (Terang)
                }
                else
                {
                    tempColor.a = 0.2f; // Opacity 20% (Gelap/Transparan)
                }

                allPhotos[i].color = tempColor;
            }
        }
    }

    // Fungsi dipanggil Vuforia saat kartu kedetek
    public void UnlockItem(string name)
    {
        // Set ke 1 artinya sudah terbuka
        PlayerPrefs.SetInt(name, 1);
        PlayerPrefs.Save();
        
        UpdateAllIcons();
        Debug.Log("Item " + name + " sekarang Unlocked!");
    }

    public void OpenPopUpIfUnlocked(int index)
    {
        if (PlayerPrefs.GetInt(itemNames[index], 0) == 1)
        {
            if (index < allPopUpPanels.Length && allPopUpPanels[index] != null)
            {
                allPopUpPanels[index].SetActive(true);
            }
        }
    }

    public void CloseAllPopUps()
    {
        foreach (GameObject panel in allPopUpPanels)
        {
            if (panel != null) panel.SetActive(false);
        }
    }
    
    // --- TAMBAHAN: Buat ngetes/reset data pas lagi development ---
    [ContextMenu("Reset Collection")]
    public void ResetData()
    {
        PlayerPrefs.DeleteAll();
        UpdateAllIcons();
    }
}