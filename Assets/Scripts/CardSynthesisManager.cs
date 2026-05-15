using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using IEnumerator = System.Collections.IEnumerator;

public class CardSynthesisManager : MonoBehaviour
{
    public static CardSynthesisManager Instance { get; private set; }

    [Header("Misi di Level Ini")]
    public List<MissionData> levelMissions;
    public float proximityThreshold = 2.0f;

    [Header("Fitur QoL: Pinyin & Alert")]
    public GameObject pinyinTextPrefab;

    [Tooltip("Jarak teks dari bagian paling atas asset 3D. Untuk roti coba 0.25 - 0.45")]
    public float pinyinHeightOffset = 0.35f;

    [Tooltip("Rotasi tambahan untuk teks. Kalau teks kebalik, coba Y = 180")]
    public Vector3 pinyinRotationOffset = Vector3.zero;

    [Tooltip("Ukuran default kalau prefab pinyinTextPrefab kosong dan script membuat TextMeshPro otomatis")]
    public Vector3 generatedPinyinScale = new Vector3(0.03f, 0.03f, 0.03f);

    public GameObject wrongCombinationAlertUI;
    public float wrongAlertDuration = 2.0f;

    [Header("Debug Status")]
    public MissionData currentMission;
    public List<ZicharaCard> cardsOnCamera = new List<ZicharaCard>();

    private int currentMissionIndex = 0;
    private bool isMissionFinished = false;
    private bool isWaitingForNextMission = false;
    private float wrongAlertCooldown = 0f;

    private List<string> completedRecipesInMission = new List<string>();
    private Dictionary<string, GameObject> activeSynthesisObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> activePinyinTexts = new Dictionary<string, GameObject>();

    private AudioSource audioSource;

    private string MissionKey => "CurrentMission_" + SceneManager.GetActiveScene().name;

    private int CurrentLevelNumber
    {
        get
        {
            string name = SceneManager.GetActiveScene().name;
            if (name.Length > 0 && char.IsDigit(name[name.Length - 1]))
                return int.Parse(name[name.Length - 1].ToString());

            return 1;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (wrongCombinationAlertUI != null)
            wrongCombinationAlertUI.SetActive(false);
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

    private void Update()
    {
        if (currentMission == null || isMissionFinished) return;

        CheckRecipes();
        UpdateObjectPositions();

        if (wrongAlertCooldown > 0)
        {
            wrongAlertCooldown -= Time.deltaTime;
        }
        else
        {
            CheckForWrongCombinations();
        }
    }

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

    private void CheckRecipes()
    {
        foreach (CardRecipe recipe in currentMission.recipes)
        {
            if (recipe == null || recipe.resultPrefab == null) continue;

            bool formed = IsRecipeFormed(recipe);
            bool isSpawned = activeSynthesisObjects.ContainsKey(recipe.recipeName);

            if (formed && !isSpawned)
            {
                SpawnSynthesis(recipe);
            }
            else if (!formed && isSpawned)
            {
                RemoveSynthesis(recipe.recipeName);
            }
        }
    }

    private bool IsRecipeFormed(CardRecipe recipe)
    {
        if (recipe.requiredCards == null || recipe.requiredCards.Count == 0)
            return false;

        List<ZicharaCard> found = new List<ZicharaCard>();

        foreach (string id in recipe.requiredCards)
        {
            ZicharaCard c = cardsOnCamera.Find(card => card.cardID == id);
            if (c == null) return false;

            found.Add(c);
        }

        Vector3 center = Vector3.zero;

        foreach (ZicharaCard card in found)
            center += card.transform.position;

        center /= found.Count;

        foreach (ZicharaCard card in found)
        {
            if (Vector3.Distance(card.transform.position, center) > proximityThreshold)
                return false;
        }

        return true;
    }

    private void SpawnSynthesis(CardRecipe recipe)
    {
        if (recipe.resultPrefab == null) return;

        Vector3 pos = CalculateCentroid(recipe.requiredCards);

        Quaternion spawnRotation = Quaternion.identity;
        ZicharaCard refCard = GetReferenceCard(recipe);

        if (refCard != null)
            spawnRotation = refCard.transform.rotation;

        GameObject obj = Instantiate(recipe.resultPrefab, pos, spawnRotation);
        activeSynthesisObjects.Add(recipe.recipeName, obj);

        SpawnPinyinText(recipe, obj);

        if (recipe.recipeSuccessSound != null)
            audioSource.PlayOneShot(recipe.recipeSuccessSound);

        if (!completedRecipesInMission.Contains(recipe.recipeName))
        {
            completedRecipesInMission.Add(recipe.recipeName);
            CheckMissionCompletion();
        }
    }

    private void SpawnPinyinText(CardRecipe recipe, GameObject targetObject)
    {
        if (string.IsNullOrWhiteSpace(recipe.pinyinText)) return;

        GameObject textObj;

        if (pinyinTextPrefab != null)
        {
            textObj = Instantiate(pinyinTextPrefab);
            textObj.name = "FloatingPinyin_" + recipe.recipeName;
            textObj.SetActive(true);
        }
        else
        {
            textObj = CreateGeneratedPinyinObject(recipe.recipeName);
        }

        // Karena kamu mau 3D text biasa ikut rotasi dunia, Billboard dimatikan otomatis.
        Billboard[] billboards = textObj.GetComponentsInChildren<Billboard>(true);
        foreach (Billboard billboard in billboards)
            billboard.enabled = false;

        Canvas canvas = textObj.GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 30;
        }

        TMP_Text tmpText = textObj.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null)
        {
            tmpText.text = recipe.pinyinText;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.enableWordWrapping = false;
            tmpText.overflowMode = TextOverflowModes.Overflow;
            tmpText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Pinyin prefab tidak punya komponen TMP_Text/TextMeshPro.");
        }

        activePinyinTexts.Add(recipe.recipeName, textObj);
        UpdatePinyinTextTransform(recipe.recipeName, targetObject, recipe);
    }

    private GameObject CreateGeneratedPinyinObject(string recipeName)
    {
        GameObject textObj = new GameObject("FloatingPinyin_" + recipeName);

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = "Pinyin";
        tmp.fontSize = 3f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.color = Color.white;

        tmp.rectTransform.sizeDelta = new Vector2(5f, 1f);

        textObj.transform.localScale = generatedPinyinScale;

        return textObj;
    }

    private void RemoveSynthesis(string recipeName)
    {
        if (activeSynthesisObjects.TryGetValue(recipeName, out GameObject obj))
        {
            if (obj != null)
                Destroy(obj);

            activeSynthesisObjects.Remove(recipeName);
        }

        if (activePinyinTexts.TryGetValue(recipeName, out GameObject textObj))
        {
            if (textObj != null)
                Destroy(textObj);

            activePinyinTexts.Remove(recipeName);
        }
    }

    private void UpdateObjectPositions()
    {
        foreach (var entry in activeSynthesisObjects)
        {
            CardRecipe recipe = currentMission.recipes.Find(r => r.recipeName == entry.Key);
            if (recipe == null || entry.Value == null) continue;

            GameObject synthesisObject = entry.Value;

            synthesisObject.transform.position = CalculateCentroid(recipe.requiredCards);

            ZicharaCard refCard = GetReferenceCard(recipe);
            if (refCard != null)
                synthesisObject.transform.rotation = refCard.transform.rotation;

            UpdatePinyinTextTransform(entry.Key, synthesisObject, recipe);
        }
    }

    private void UpdatePinyinTextTransform(string recipeName, GameObject targetObject, CardRecipe recipe)
    {
        if (!activePinyinTexts.TryGetValue(recipeName, out GameObject textObj)) return;
        if (textObj == null || targetObject == null) return;

        Vector3 labelPosition = GetPinyinWorldPosition(targetObject);
        Quaternion labelRotation = targetObject.transform.rotation * Quaternion.Euler(pinyinRotationOffset);

        textObj.transform.position = labelPosition;
        textObj.transform.rotation = labelRotation;

        TMP_Text tmpText = textObj.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null && tmpText.text != recipe.pinyinText)
            tmpText.text = recipe.pinyinText;
    }

    private Vector3 GetPinyinWorldPosition(GameObject targetObject)
    {
        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return targetObject.transform.position + Vector3.up * pinyinHeightOffset;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return new Vector3(
            bounds.center.x,
            bounds.max.y + pinyinHeightOffset,
            bounds.center.z
        );
    }

    private ZicharaCard GetReferenceCard(CardRecipe recipe)
    {
        if (recipe.requiredCards == null || recipe.requiredCards.Count == 0)
            return null;

        return cardsOnCamera.Find(c => c.cardID == recipe.requiredCards[0]);
    }

    private Vector3 CalculateCentroid(List<string> reqCards)
    {
        if (reqCards == null || reqCards.Count == 0)
            return Vector3.zero;

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

    private void CheckForWrongCombinations()
    {
        if (cardsOnCamera.Count < 2) return;

        for (int i = 0; i < cardsOnCamera.Count; i++)
        {
            for (int j = i + 1; j < cardsOnCamera.Count; j++)
            {
                if (Vector3.Distance(cardsOnCamera[i].transform.position, cardsOnCamera[j].transform.position) <= proximityThreshold)
                {
                    bool isPairValidInAnyRecipe = false;

                    foreach (CardRecipe recipe in currentMission.recipes)
                    {
                        if (recipe.requiredCards.Contains(cardsOnCamera[i].cardID) &&
                            recipe.requiredCards.Contains(cardsOnCamera[j].cardID))
                        {
                            isPairValidInAnyRecipe = true;
                            break;
                        }
                    }

                    if (!isPairValidInAnyRecipe)
                    {
                        TriggerWrongCombinationAlert();
                        return;
                    }
                }
            }
        }
    }

    private void TriggerWrongCombinationAlert()
    {
        if (wrongCombinationAlertUI != null && !wrongCombinationAlertUI.activeSelf)
        {
            Debug.Log("Kombinasi Salah Ditemukan!");
            StartCoroutine(ShowAlertRoutine());
        }
    }

    private IEnumerator ShowAlertRoutine()
    {
        wrongCombinationAlertUI.SetActive(true);
        wrongAlertCooldown = wrongAlertDuration + 1.0f;

        yield return new WaitForSeconds(wrongAlertDuration);

        wrongCombinationAlertUI.SetActive(false);
    }

    private void CheckMissionCompletion()
    {
        if (currentMission == null) return;

        bool completed = false;

        if (currentMission.completionRule == MissionCompletionRule.RequireAllRecipes)
        {
            completed = completedRecipesInMission.Count >= currentMission.recipes.Count;
        }
        else if (currentMission.completionRule == MissionCompletionRule.RequireAnyRecipe)
        {
            completed = completedRecipesInMission.Count >= 1;
        }

        if (completed && !isWaitingForNextMission)
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
            isMissionFinished = true;

            if (currentMission.missionFinishSound != null)
                audioSource.PlayOneShot(currentMission.missionFinishSound);

            int thisLevel = CurrentLevelNumber;
            int lastCleared = PlayerPrefs.GetInt("LastClearedLevel", 1);

            if (thisLevel >= lastCleared)
                PlayerPrefs.SetInt("LastClearedLevel", thisLevel + 1);

            PlayerPrefs.SetInt(MissionKey, 0);
            PlayerPrefs.Save();

            StoryController story = FindFirstObjectByType<StoryController>();
            if (story != null)
                story.SetupMissionUI(99);
        }
    }

    private void LoadMission(int index)
    {
        foreach (var entry in activeSynthesisObjects)
        {
            if (entry.Value != null)
                Destroy(entry.Value);
        }

        activeSynthesisObjects.Clear();

        foreach (var entry in activePinyinTexts)
        {
            if (entry.Value != null)
                Destroy(entry.Value);
        }

        activePinyinTexts.Clear();

        currentMission = levelMissions[index];
        currentMissionIndex = index;
        completedRecipesInMission.Clear();

        PlayerPrefs.SetInt(MissionKey, index);
        PlayerPrefs.Save();

        StoryController story = FindFirstObjectByType<StoryController>();
        if (story != null)
            story.SetupMissionUI(index + 1);

        if (currentMission.missionStartSound != null)
            audioSource.PlayOneShot(currentMission.missionStartSound);
    }

    public void GoToLevelSelection()
    {
        SceneManager.LoadScene("LevelSelection");
    }

    private void ValidateMissions()
    {
        for (int i = 0; i < levelMissions.Count; i++)
        {
            if (levelMissions[i] == null) continue;

            foreach (CardRecipe recipe in levelMissions[i].recipes)
            {
                if (recipe.resultPrefab == null)
                {
                    Debug.LogError($"Mission '{levelMissions[i].name}' → Recipe '{recipe.recipeName}' has no resultPrefab!");
                }

                if (string.IsNullOrWhiteSpace(recipe.pinyinText))
                {
                    Debug.LogWarning($"Mission '{levelMissions[i].name}' → Recipe '{recipe.recipeName}' belum punya pinyinText.");
                }
            }
        }
    }
}