using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
            Destroy(activeSynthesisObjects[recipeName]);
            activeSynthesisObjects.Remove(recipeName);
        }
    }

    private void UpdateObjectPositions()
    {
        foreach (var entry in activeSynthesisObjects)
        {
            string rName = entry.Key;
            GameObject obj = entry.Value;

            CardRecipe recipe = currentMission.recipes.Find(r => r.recipeName == rName);
            if (recipe != null)
            {
                // Update posisi ke tengah-tengah kartu
                obj.transform.position = CalculateCentroid(recipe.requiredCards);

                // Update rotasi biar ngikutin kartu pertama (biar nggak kaku)
                ZicharaCard firstCard = cardsOnCamera.Find(c => c.cardID == recipe.requiredCards[0]);
                if (firstCard != null)
                {
                    obj.transform.rotation = firstCard.transform.rotation;
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

    // --- SISANYA (Fungsi Misi) Tetap Sama ---
    private void CheckMissionCompletion() { /* ... kode lama ... */ }
    public void LoadNextMissionInQueue() { /* ... kode lama ... */ }
    private void LoadMission(int index) { /* ... kode lama ... */ }
}