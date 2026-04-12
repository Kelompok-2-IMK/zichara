using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    [Header("Koleksi Settings")]
    // List semua Rectangle/Container di halaman Legend
    public Image[] allPhotos; 
    // Nama unik (bao, shui, dll) - Harus sama dengan di Vuforia
    public string[] itemNames; 

    [Header("UI Pop Up Settings")]
    // List semua panel pop-up (PopUpBao, PopUpShui, dll)
    public GameObject[] allPopUpPanels; 

    void Start()
    {
        // Pastikan semua pop-up tertutup saat mulai
        CloseAllPopUps();
        // Update tampilan ikon (mana yang masih gelap/terang)
        UpdateAllIcons();
    }

    public void UpdateAllIcons()
    {
        // Pengaman agar tidak error di scene scan yang tidak ada UI-nya
        if (allPhotos == null || allPhotos.Length == 0) return;

        for (int i = 0; i < allPhotos.Length; i++)
        {
            // Mengambil Image ikon di dalam Child (pakai GetChild(0))
            Image iconImage = allPhotos[i].transform.GetChild(0).GetComponent<Image>();
            
            if (iconImage != null)
            {
                if (PlayerPrefs.GetInt(itemNames[i], 0) == 1)
                {
                    iconImage.color = new Color(1, 1, 1, 1f); // Terang
                }
                else
                {
                    iconImage.color = new Color(1, 1, 1, 0.1f); // Gelap
                }
            }
        }
    }

    // Fungsi dipanggil Vuforia saat kartu kedetek (Hanya untuk Unlock)
    public void UnlockItem(string name)
    {
        if (PlayerPrefs.GetInt(name, 0) == 0)
        {
            PlayerPrefs.SetInt(name, 1);
            PlayerPrefs.Save();
            UpdateAllIcons();
            Debug.Log(name + " berhasil dibuka!");
        }
    }

    // Fungsi dipanggil saat tombol di Legend diklik
    public void OpenPopUpIfUnlocked(int index)
    {
        string name = itemNames[index];

        // Cek apakah item ini sudah pernah di-scan
        if (PlayerPrefs.GetInt(name, 0) == 1)
        {
            if (allPopUpPanels.Length > index && allPopUpPanels[index] != null)
            {
                allPopUpPanels[index].SetActive(true);
            }
        }
        else
        {
            Debug.Log("Item ini masih terkunci! Scan kartu dulu.");
        }
    }

    // Fungsi untuk tombol Close di pop-up
    public void CloseAllPopUps()
    {
        if (allPopUpPanels == null) return;
        foreach (GameObject panel in allPopUpPanels)
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}