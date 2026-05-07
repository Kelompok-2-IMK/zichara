using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using IEnumerator = System.Collections.IEnumerator;

public class CardSynthesisManager : MonoBehaviour
{
    public static CardSynthesisManager Instance { get; private set; }

    [Header("Misi di Level Ini")]
    public List<MissionData> levelMissions;
    public float proximityThreshold = 2.0f;

    [Header("Debug Status")]
    public MissionData currentMission;
    public List<ZicharaCard> cardsOnCamera = new List<ZicharaCard>();

    private int currentMissionIndex = 0;
    private bool isMissionFinished = false;
    private bool isWaitingForNextMission = false;
    private List<string> completedRecipesInMission = new List<string>();
    private Dictionary<string, GameObject> activeSynthesisObjects = new Dictionary<string, GameObject>();

    // Nama scene ini, untuk key PlayerPrefs
    private string MissionKey => "CurrentMission_" + SceneManager.GetActiveScene().name;

    // Nomor level berdasarkan nama scene (Scan_Story1 → 1, dst)
    private int CurrentLevelNumber
    {
        get
        {
            string name = SceneManager.GetActiveScene().name;
            // Ambil angka terakhir dari nama scene
            if (name.Length > 0 && char.IsDigit(name[name.Length - 1]))
                return int.Parse(name[name.Length - 1].ToString());
            return 1;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ValidateMissions();

        if (levelMissions.Count > 0)
        {
            int savedIndex = PlayerPrefs.GetInt(MissionKey, 0);
            if (savedIndex >= levelMissions.Count) savedIndex = 0;
            LoadMission(savedIndex);
        }
    }

    // ── Kartu masuk/keluar kamera ──────────────────────────────────────────

    public void AddActiveCard(ZicharaCard card)
    {
        if (!cardsOnCamera.Contains(card))
        {
            cardsOnCamera.Add(card);
            Debug.Log("Kartu Terdeteksi: " + card.cardID);
        }
    }

    public void RemoveActiveCard(ZicharaCard card)
    {
        if (cardsOnCamera.Contains(card))
        {
            cardsOnCamera.Remove(card);
            Debug.Log("Kartu Hilang: " + card.cardID);
        }
    }

    // ── Update loop ────────────────────────────────────────────────────────

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
            if (recipe == null || recipe.resultPrefab == null) continue;

            bool formed = IsRecipeFormed(recipe);
            bool isSpawned = activeSynthesisObjects.ContainsKey(recipe.recipeName);

            if (formed && !isSpawned)
                SpawnSynthesis(recipe);
            else if (!formed && isSpawned)
                RemoveSynthesis(recipe.recipeName);
        }
    }

    private bool IsRecipeFormed(CardRecipe recipe)
    {
        List<ZicharaCard> found = new List<ZicharaCard>();

        foreach (string id in recipe.requiredCards)
        {
            ZicharaCard c = cardsOnCamera.Find(card => card.cardID == id);
            if (c == null) return false;
            found.Add(c);
        }

        for (int i = 0; i < found.Count; i++)
            for (int j = i + 1; j < found.Count; j++)
                if (Vector3.Distance(found[i].transform.position, found[j].transform.position) > proximityThreshold)
                    return false;

        return true;
    }

    private void SpawnSynthesis(CardRecipe recipe)
    {
        if (recipe.resultPrefab == null)
        {
            Debug.LogError($"resultPrefab NULL untuk resep '{recipe.recipeName}'!");
            return;
        }

        Vector3 pos = CalculateCentroid(recipe.requiredCards);
        GameObject obj = Instantiate(recipe.resultPrefab, pos, Quaternion.identity);
        activeSynthesisObjects.Add(recipe.recipeName, obj);

        if (!completedRecipesInMission.Contains(recipe.recipeName))
        {
            completedRecipesInMission.Add(recipe.recipeName);
            CheckMissionCompletion();
        }
    }

    private void RemoveSynthesis(string recipeName)
    {
        if (activeSynthesisObjects.TryGetValue(recipeName, out GameObject obj))
        {
            if (obj != null) Destroy(obj);
            activeSynthesisObjects.Remove(recipeName);
        }
    }

    private void UpdateObjectPositions()
    {
        foreach (var entry in activeSynthesisObjects)
        {
            CardRecipe recipe = currentMission.recipes.Find(r => r.recipeName == entry.Key);
            if (recipe == null || entry.Value == null) continue;

            entry.Value.transform.position = CalculateCentroid(recipe.requiredCards);

            ZicharaCard refCard = cardsOnCamera.Find(c => c.cardID == recipe.requiredCards[0]);
            if (refCard != null)
                entry.Value.transform.rotation = refCard.transform.rotation;
        }
    }

    private Vector3 CalculateCentroid(List<string> reqCards)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (string id in reqCards)
        {
            ZicharaCard c = cardsOnCamera.Find(card => card.cardID == id);
            if (c != null) { sum += c.transform.position; count++; }
        }
        return count > 0 ? sum / count : Vector3.zero;
    }

    // ── Misi selesai ───────────────────────────────────────────────────────

    private void CheckMissionCompletion()
    {
        if (completedRecipesInMission.Count >= currentMission.recipes.Count && !isWaitingForNextMission)
        {
            isWaitingForNextMission = true;
            Debug.Log($"Misi {currentMission.name} Selesai! Menunggu 5 detik...");
            StartCoroutine(WaitAndLoadNextMission(5f));
        }
    }

    private IEnumerator WaitAndLoadNextMission(float delay)
    {
        yield return new WaitForSeconds(delay);
        isWaitingForNextMission = false;
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
            // Semua misi di level ini selesai
            isMissionFinished = true;
            Debug.Log("SEMUA MISI LEVEL INI SELESAI!");

            // Unlock level berikutnya
            int thisLevel = CurrentLevelNumber;
            int lastCleared = PlayerPrefs.GetInt("LastClearedLevel", 1);
            if (thisLevel >= lastCleared)
            {
                PlayerPrefs.SetInt("LastClearedLevel", thisLevel + 1);
            }

            // Reset progress misi level ini (kalau dimainin ulang mulai dari misi 1)
            PlayerPrefs.SetInt(MissionKey, 0);
            PlayerPrefs.Save();

            // Tampilkan popup finish via StoryController
            StoryController story = FindFirstObjectByType<StoryController>();
            if (story != null) story.SetupMissionUI(99); // 99 = kode "finish"
        }
    }

    // ── Load misi ─────────────────────────────────────────────────────────

    private void LoadMission(int index)
    {
        foreach (var entry in activeSynthesisObjects)
            if (entry.Value != null) Destroy(entry.Value);
        activeSynthesisObjects.Clear();

        currentMission = levelMissions[index];
        currentMissionIndex = index;
        completedRecipesInMission.Clear();

        // Simpan progress
        PlayerPrefs.SetInt(MissionKey, index);
        PlayerPrefs.Save();

        StoryController story = FindFirstObjectByType<StoryController>();
        if (story != null) story.SetupMissionUI(index + 1);

        Debug.Log($"Memulai Misi [{index}]: {currentMission.name}");
    }

    // ── Tombol "Kembali ke Level Selection" (dipanggil dari popup finish) ──

    public void GoToLevelSelection()
    {
        SceneManager.LoadScene("LevelSelection");
    }

    // ── Validasi di Start ─────────────────────────────────────────────────

    private void ValidateMissions()
    {
        for (int i = 0; i < levelMissions.Count; i++)
        {
            if (levelMissions[i] == null)
            {
                Debug.LogError($"levelMissions[{i}] is NULL!");
                continue;
            }
            foreach (CardRecipe recipe in levelMissions[i].recipes)
            {
                if (recipe.resultPrefab == null)
                    Debug.LogError($"Mission '{levelMissions[i].name}' → Recipe '{recipe.recipeName}' has no resultPrefab!");
            }
        }
    }
}