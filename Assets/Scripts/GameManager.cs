using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Pola Singleton: Memastikan hanya ada satu GameManager di seluruh scene
    public static GameManager Instance { get; private set; }

    [Header("Status Pemain Saat Ini")]
    public PlayerData currentPlayer;

    private void Awake()
    {
        // Setup Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        
        // Load data json saat game pertama kali nyala
        currentPlayer = SaveManager.LoadData();
    }

    // --- FUNGSI-FUNGSI BANTUAN UNTUK SCRIPT LAIN ---

    // Dipanggil saat Chichi berhasil scan misi (misal: Nemu Roti)
    public void CompleteMission(string missionID, string itemID)
    {
        if (!currentPlayer.completedMissions.Contains(missionID))
        {
            currentPlayer.completedMissions.Add(missionID);
        }

        if (!string.IsNullOrEmpty(itemID) && !currentPlayer.collectedItems.Contains(itemID))
        {
            // Masukkan ke halaman Legends
            currentPlayer.collectedItems.Add(itemID); 
        }

        // Simpan otomatis setiap ada progress
        SaveManager.SaveData(currentPlayer); 
    }

    // Dipanggil saat semua misi di suatu level beres
    public void UnlockNextLevel(int newLevel)
    {
        if (newLevel > currentPlayer.lastUnlockedLevel)
        {
            currentPlayer.lastUnlockedLevel = newLevel;
            SaveManager.SaveData(currentPlayer);
        }
    }
}