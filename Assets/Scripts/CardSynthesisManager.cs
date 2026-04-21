using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardSynthesisManager : MonoBehaviour
{
    public static CardSynthesisManager Instance { get; private set; }

    [Header("Antrean Level 1")]
    [Tooltip("Masukkan Misi 1, lalu Misi 2 ke daftar ini secara berurutan")]
    public List<MissionData> levelMissions; 
    public float proximityThreshold = 2.0f;

    [Header("Integrasi UI (Tugas Temanmu)")]
    public UnityEvent<MissionData> OnMissionStarted; // Trigger saat misi baru mulai
    public UnityEvent<MissionData> OnMissionSuccess; // Trigger saat misi selesai
    
    // Opsional: Untuk coret checklist tiap 1 barang ketemu di Misi 1
    public UnityEvent<string> OnItemDiscovered; 

    [Header("Debug Status")]
    public MissionData currentMission;
    public List<ZicharaCard> cardsOnCamera = new List<ZicharaCard>();
    
    private int currentMissionIndex = 0;
    private bool isMissionFinished = false;
    
    // Ingatan tentang resep apa yang sudah dibuat dan objek 3D yang muncul
    private List<string> completedRecipesInMission = new List<string>();
    private List<GameObject> active3DObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Mulai antrean level dari Misi 1 (Index 0)
        if (levelMissions.Count > 0)
        {
            LoadMission(0);
        }
    }

    public void AddActiveCard(ZicharaCard card)
    {
        if (!cardsOnCamera.Contains(card)) cardsOnCamera.Add(card);
    }

    public void RemoveActiveCard(ZicharaCard card)
    {
        if (cardsOnCamera.Contains(card)) cardsOnCamera.Remove(card);
    }

    private void Update()
    {
        if (currentMission == null || isMissionFinished) return;
        CheckRecipes();
    }

    private void CheckRecipes()
    {
        // Mengecek semua resep yang ada di misi saat ini
        foreach (CardRecipe recipe in currentMission.recipes)
        {
            // Abaikan jika resep ini sudah berhasil dibuat sebelumnya (misal Roti udah, tinggal Tas)
            if (completedRecipesInMission.Contains(recipe.recipeName)) continue;

            if (IsRecipeFormed(recipe))
            {
                ExecuteSynthesis(recipe);
                break; // Hanya buat 1 objek per frame untuk mencegah lag
            }
        }
    }

    private bool IsRecipeFormed(CardRecipe recipe)
    {
        List<ZicharaCard> cardsForThisRecipe = new List<ZicharaCard>();

        // 1. Cek apakah kartu untuk resep ini ada di layar
        foreach (string reqCard in recipe.requiredCards)
        {
            ZicharaCard foundCard = cardsOnCamera.Find(c => c.cardID == reqCard);
            if (foundCard == null) return false; // Kurang kartu
            
            cardsForThisRecipe.Add(foundCard);
        }

        // 2. Cek jarak (HANYA antar kartu yang jadi bagian dari resep ini)
        for (int i = 0; i < cardsForThisRecipe.Count; i++)
        {
            for (int j = i + 1; j < cardsForThisRecipe.Count; j++)
            {
                float dist = Vector3.Distance(cardsForThisRecipe[i].transform.position, cardsForThisRecipe[j].transform.position);
                if (dist > proximityThreshold) return false;
            }
        }

        return true; // Resep terbentuk!
    }

    private void ExecuteSynthesis(CardRecipe recipe)
    {
        // Cari titik tengah (Centroid) dari kartu-kartu yang spesifik membentuk resep ini
        Vector3 spawnPosition = Vector3.zero;
        int cardCount = 0;
        foreach (string reqCard in recipe.requiredCards)
        {
            ZicharaCard card = cardsOnCamera.Find(c => c.cardID == reqCard);
            if (card != null)
            {
                spawnPosition += card.transform.position;
                cardCount++;
            }
        }
        spawnPosition /= cardCount;

        // Munculkan 3D
        if (recipe.resultPrefab != null)
        {
            GameObject newObj = Instantiate(recipe.resultPrefab, spawnPosition, Quaternion.identity);
            active3DObjects.Add(newObj); // Simpan ke memori untuk di-cleanup nanti
        }

        completedRecipesInMission.Add(recipe.recipeName);
        OnItemDiscovered?.Invoke(recipe.recipeName); // Kasih tau UI buat nyoret checklist

        // Cek Kondisi Menang
        CheckMissionCompletion();
    }

    private void CheckMissionCompletion()
    {
        if (currentMission.completionRule == MissionCompletionRule.RequireAnyRecipe)
        {
            isMissionFinished = true; // Cukup 1 resep jadi, misi sukses (Contoh: Sepeda ATAU Kereta)
        }
        else if (currentMission.completionRule == MissionCompletionRule.RequireAllRecipes)
        {
            // Sukses jika jumlah resep yang dibuat == jumlah resep yang diminta
            if (completedRecipesInMission.Count >= currentMission.recipes.Count)
            {
                isMissionFinished = true; 
            }
        }

        if (isMissionFinished)
        {
            GameManager.Instance.CompleteMission(currentMission.missionID, "");
            OnMissionSuccess?.Invoke(currentMission);
        }
    }

    // --- FUNGSI UNTUK PINDAH MISI (Dipanggil oleh Tombol Lanjut teman UI-mu) ---
    public void LoadNextMissionInQueue()
    {
        // 1. CLEANUP: Hapus semua objek 3D dari misi sebelumnya biar layar bersih
        foreach (GameObject obj in active3DObjects) Destroy(obj);
        active3DObjects.Clear();
        completedRecipesInMission.Clear();
        cardsOnCamera.Clear(); // Bersihkan ingatan kamera

        // 2. Maju ke misi berikutnya
        currentMissionIndex++;
        if (currentMissionIndex < levelMissions.Count)
        {
            LoadMission(currentMissionIndex);
        }
        else
        {
            Debug.Log("SEMUA MISI DI LEVEL INI SELESAI!");
            GameManager.Instance.UnlockNextLevel(2); // Buka level 2 di JSON
            // Panggil event UI untuk pindah layar ke Level Selection
        }
    }

    private void LoadMission(int index)
    {
        currentMission = levelMissions[index];
        isMissionFinished = false;
        
        // Panggil UI temanmu untuk memunculkan popup cerita awal
        OnMissionStarted?.Invoke(currentMission); 
    }
}