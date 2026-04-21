using UnityEngine;
using TMPro; // Wajib untuk TextMeshPro
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Komponen Popup Cerita")]
    public GameObject popupPanel; // Panel background gelap
    public TextMeshProUGUI popupTitleText;
    public TextMeshProUGUI popupBodyText;
    public Button popupCloseButton;

    [Header("Komponen Checklist")]
    public GameObject checklistPanel;
    public TextMeshProUGUI checklistText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Pasang fungsi tutup ke tombol Lanjut/Tutup
        popupCloseButton.onClick.AddListener(ClosePopup);
    }

    // --- FUNGSI MUNCULKAN POPUP DINAMIS ---
    public void ShowPopup(string title, string storyMessage)
    {
        // Suntikkan teks baru ke UI
        popupTitleText.text = title;
        popupBodyText.text = storyMessage;
        
        // Munculkan layarnya
        popupPanel.SetActive(true);
        
        // Sembunyikan checklist saat cerita berjalan
        checklistPanel.SetActive(false); 
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
        //checklistPanel.SetActive(true); // Tampilkan lagi checklistnya
    }

    // --- FUNGSI UPDATE CHECKLIST ---
    public void UpdateChecklist(string currentTask, bool isDone)
    {
        if(isDone)
        {
            // Memberikan efek coret (strikethrough) pada teks
            checklistText.text = "<s>[x] " + currentTask + "</s>";
            checklistText.color = Color.gray;
        }
        else
        {
            checklistText.text = "[ ] " + currentTask;
            checklistText.color = Color.white;
        }
    }

    // Fungsi ini yang akan dipanggil oleh ButtonCerita
    public void TogglePopupCerita()
    {
        // 1. Kalau popup sedang menyala, kita matikan (Tutup)
        if (popupPanel.activeSelf)
        {
            ClosePopup();
        }
        // 2. Kalau popup sedang mati, kita nyalakan (Buka)
        else
        {
            // Ambil data misi yang sedang berjalan dari CardSynthesisManager
            MissionData misiAktif = CardSynthesisManager.Instance.currentActiveMission;
            
            // Cek apakah ada misi yang aktif
            if (misiAktif != null)
            {
                // Suntikkan teks startStoryText dari MissionData ke Popup
                ShowPopup(misiAktif.missionTitle, misiAktif.startStoryText);
            }
            else
            {
                Debug.LogWarning("Tidak ada misi aktif di CardSynthesisManager!");
            }
        }
    }
}