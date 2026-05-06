using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using IEnumerator = System.Collections.IEnumerator;

public class CardSynthesisManager : MonoBehaviour
{
    public static CardSynthesisManager Instance { get; private set; }

    [Header("Antrean Level 1")]
    public List<MissionData> levelMissions; 
    public float proximityThreshold = 2.0f;

    [Header("Debug Status")]
    public MissionData currentMission;
    public List<ZicharaCard> cardsOnCamera = new List<ZicharaCard>();
    
    private int currentMissionIndex = 0;
    private bool isMissionFinished = false;
    private bool isWaitingForNextMission = false;
    private List<string> completedRecipesInMission = new List<string>();
    
    // Kita simpan objek yang aktif di sini
    private Dictionary<string, GameObject> activeSynthesisObjects = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (levelMissions.Count > 0) LoadMission(0);
    }

    // Fungsi ini dipanggil otomatis oleh ZicharaCard
    public void AddActiveCard(ZicharaCard card)
    {
        if (!cardsOnCamera.Contains(card))
        {
            cardsOnCamera.Add(card);
            Debug.Log("Kartu Terdeteksi: " + card.cardID);
        }
    }

    // Fungsi ini dipanggil otomatis oleh ZicharaCard
    public void RemoveActiveCard(ZicharaCard card)
    {
        if (cardsOnCamera.Contains(card))
        {
            cardsOnCamera.Remove(card);
            Debug.Log("Kartu Hilang: " + card.cardID);
        }
    }

    private void Update()
    {
        if (currentMission == null || isMissionFinished) return;
        
        CheckRecipes();
        UpdateObjectPositions();
    }

    private void CheckRecipes()
    {
        foreach (CardRecipe recipe in currentMission.recipes)
        {
            bool formed = IsRecipeFormed(recipe);
            bool isSpawned = activeSynthesisObjects.ContainsKey(recipe.recipeName);

            // LOGIKA COMBO: Jika kartu lengkap & jarak dekat, tapi belum ada barangnya
            if (formed && !isSpawned)
            {
                SpawnSynthesis(recipe);
            }
            // LOGIKA HILANG: Jika kartu tidak lengkap/jauh, tapi barangnya masih ada
            else if (!formed && isSpawned)
            {
                RemoveSynthesis(recipe.recipeName);
            }
        }
    }

    private bool IsRecipeFormed(CardRecipe recipe)
    {
        List<ZicharaCard> foundCardsForRecipe = new List<ZicharaCard>();

        // 1. Cek keberadaan semua kartu yang dibutuhkan
        foreach (string id in recipe.requiredCards)
        {
            ZicharaCard c = cardsOnCamera.Find(card => card.cardID == id);
            if (c == null) return false; // Ada satu kartu yang nggak kelihatan
            foundCardsForRecipe.Add(c);
        }

        // 2. Cek jarak antar kartu (Proximity)
        for (int i = 0; i < foundCardsForRecipe.Count; i++)
        {
            for (int j = i + 1; j < foundCardsForRecipe.Count; j++)
            {
                if (Vector3.Distance(foundCardsForRecipe[i].transform.position, foundCardsForRecipe[j].transform.position) > proximityThreshold)
                    return false; // Kartunya kejauhan
            }
        }

        return true;
    }

    private void SpawnSynthesis(CardRecipe recipe)
    {
        Vector3 pos = CalculateCentroid(recipe.requiredCards);
        GameObject obj = Instantiate(recipe.resultPrefab, pos, Quaternion.identity);
        activeSynthesisObjects.Add(recipe.recipeName, obj);

        // Tandai misi selesai jika perlu
        if (!completedRecipesInMission.Contains(recipe.recipeName))
        {
            completedRecipesInMission.Add(recipe.recipeName);
            CheckMissionCompletion();
        }
    }

    private void RemoveSynthesis(string recipeName)
    {
        if (activeSynthesisObjects.ContainsKey(recipeName))
        {
            GameObject objToRemove = activeSynthesisObjects[recipeName];
            if (objToRemove != null)
            {
                Destroy(objToRemove);
            }
            activeSynthesisObjects.Remove(recipeName);
            Debug.Log("Menghapus objek: " + recipeName);
        }
    }

    private void UpdateObjectPositions()
    {
        foreach (var entry in activeSynthesisObjects)
        {
            string rName = entry.Key;
            GameObject obj = entry.Value;

            // Cari data resep yang sesuai dengan objek yang sedang aktif ini
            CardRecipe recipe = currentMission.recipes.Find(r => r.recipeName == rName);
            
            if (recipe != null && obj != null)
            {
                // 1. Update posisi ke tengah-tengah kartu yang KHUSUS dibutuhkan resep ini
                obj.transform.position = CalculateCentroid(recipe.requiredCards);

                // 2. Update rotasi agar mengikuti kartu PERTAMA dari resep tersebut
                // Jangan pakai cardsOnCamera[0], tapi cari kartu dari list requiredCards-nya
                ZicharaCard referenceCard = cardsOnCamera.Find(c => c.cardID == recipe.requiredCards[0]);
                
                if (referenceCard != null)
                {
                    obj.transform.rotation = referenceCard.transform.rotation;
                }
            }
        }
    }

    private Vector3 CalculateCentroid(List<string> reqCards)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (string id in reqCards)
        {
            ZicharaCard c = cardsOnCamera.Find(card => card.cardID == id);
            if (c != null)
            {
                sum += c.transform.position;
                count++;
            }
        }
        return count > 0 ? sum / count : Vector3.zero;
    }

    private void CheckMissionCompletion()
    {
        // Cek apakah semua resep selesai DAN pastikan kita tidak sedang dalam masa tunggu delay
        if (completedRecipesInMission.Count >= currentMission.recipes.Count && !isWaitingForNextMission)
        {
            isWaitingForNextMission = true; // Kunci agar tidak terpanggil dua kali
            Debug.Log("Misi " + currentMission.name + " Selesai! Menunggu 5 detik...");
            StartCoroutine(WaitAndLoadNextMission(5f)); 
        }
    }

    private IEnumerator WaitAndLoadNextMission(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        isWaitingForNextMission = false; // Reset pengaman
        LoadNextMissionInQueue();
    }

    public void LoadNextMissionInQueue()
    {
        currentMissionIndex++;

        if (currentMissionIndex < levelMissions.Count)
        {
            LoadMission(currentMissionIndex);
        }
        else
        {
            // SEMUA MISI SELESAI
            isMissionFinished = true;
            Debug.Log("SEMUA MISI DI LEVEL INI BERHASIL!");

            // Panggil StoryController untuk nampilin popup finish
            StoryController story = FindFirstObjectByType<StoryController>();
            if(story != null)
            {
                story.SetupMissionUI(3); // Angka 3 untuk memicu kondisi finish
            }
        }
    }

    private void LoadMission(int index)
    {
        // --- TAMBAHAN WAJIB: Hancurkan semua objek yang sedang aktif ---
        foreach (var entry in activeSynthesisObjects)
        {
            if (entry.Value != null) 
            {
                Destroy(entry.Value);
            }
        }
        activeSynthesisObjects.Clear(); 
        // --------------------------------------------------------------

        currentMission = levelMissions[index];
        currentMissionIndex = index;
        completedRecipesInMission.Clear();
        
        StoryController story = FindFirstObjectByType<StoryController>();
        if(story != null)
        {
            story.SetupMissionUI(index + 1);
        }

        Debug.Log("Memulai Misi: " + currentMission.name);
    }

    public void OnFinishLevel2()
    {
        // Simpan bahwa level 2 sudah selesai (berarti level 3 sekarang terbuka)
        PlayerPrefs.SetInt("LastClearedLevel", 3);
        PlayerPrefs.Save();
        
        // Pindah ke scene level selection
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level");
    }
}