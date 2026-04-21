using System.Collections.Generic;
using UnityEngine;

public class CardSynthesisManager : MonoBehaviour
{
    public static CardSynthesisManager Instance { get; private set; }

    [Header("Pengaturan Misi")]
    public MissionData currentActiveMission; // Misi yang sedang jalan
    public float proximityThreshold = 0.15f; // Jarak maksimal antar kartu (dalam meter/unit Unity)

    [Header("Debug Status (Jangan Diisi)")]
    public List<Card> cardsOnCamera = new List<Card>();

    private bool missionAlreadyCompleted = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- FUNGSI DARI PRAJURIT ---
    public void AddActiveCard(Card card)
    {
        if (!cardsOnCamera.Contains(card)) cardsOnCamera.Add(card);
    }

    public void RemoveActiveCard(Card card)
    {
        if (cardsOnCamera.Contains(card)) cardsOnCamera.Remove(card);
    }

    // --- LOGIKA PENGGABUNGAN (JANTUNG AR) ---
    private void Update()
    {
        // Jika tidak ada misi, atau misi sudah beres, jangan lakukan apa-apa
        if (currentActiveMission == null || missionAlreadyCompleted) return;

        // Cek apakah jumlah kartu di layar sudah memenuhi syarat misi
        if (cardsOnCamera.Count >= currentActiveMission.requiredCards.Count)
        {
            CheckCardCombination();
        }
    }

    private void CheckCardCombination()
    {
        // 1. Pastikan semua kartu yang dibutuhkan misi ADA di depan kamera
        foreach (string reqCard in currentActiveMission.requiredCards)
        {
            bool cardFound = false;
            foreach (Card activeCard in cardsOnCamera)
            {
                if (activeCard.cardID == reqCard) cardFound = true;
            }
            if (!cardFound) return; // Ada kartu yang kurang, batalkan!
        }

        // 2. Jika semua kartu lengkap, Cek Jaraknya (Proximity)
        if (AreCardsCloseEnough())
        {
            missionAlreadyCompleted = true; // Kunci agar tidak terpanggil 2x
            ExecuteSynthesis();
        }
    }

    private bool AreCardsCloseEnough()
    {
        // Mengecek jarak antara kartu pertama dengan kartu-kartu lainnya
        // Cocok untuk kombinasi 2 maupun 3 kartu sekaligus!
        Vector3 centerPoint = cardsOnCamera[0].transform.position;

        for (int i = 1; i < cardsOnCamera.Count; i++)
        {
            float distance = Vector3.Distance(centerPoint, cardsOnCamera[i].transform.position);
            if (distance > proximityThreshold)
            {
                return false; // Ada kartu yang kejauhan!
            }
        }
        return true; // Semua kartu berdekatan!
    }

    private void ExecuteSynthesis()
    {
        Debug.Log("SINTESIS BERHASIL! Memunculkan 3D...");

        // Cari titik tengah (Centroid) untuk memunculkan objek 3D Roti/Transportasi
        Vector3 spawnPosition = Vector3.zero;
        foreach (Card card in cardsOnCamera)
        {
            spawnPosition += card.transform.position;
        }
        spawnPosition /= cardsOnCamera.Count;

        // Munculkan Objek 3D Roti/Jam Tangan dll di titik tengah kartu
        if (currentActiveMission.resultPrefab != null)
        {
            Instantiate(currentActiveMission.resultPrefab, spawnPosition, Quaternion.identity);
        }

        // --- UPDATE DATA & UI ---
        // Panggil GameManager untuk save JSON
        GameManager.Instance.CompleteMission(currentActiveMission.missionID, currentActiveMission.resultPrefab?.name);
        
        // Panggil UIManager untuk coret Checklist dan munculkan PopUp Berhasil!
        UIManager.Instance.UpdateChecklist("Cari " + currentActiveMission.missionTitle, true);
        UIManager.Instance.ShowPopup("Berhasil!", currentActiveMission.endStoryText);
    }
}