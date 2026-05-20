using System.Collections.Generic;
using UnityEngine;

public class FreeplaySynthesisManager : MonoBehaviour
{
    public static FreeplaySynthesisManager Instance { get; private set; }

    [Header("Data Combo (Multi-Kartu)")]
    [SerializeField] private List<MissionData> allMissionsData;

    [Header("Data Satuan (Banyak Kartu)")]
    [Tooltip("Tarik SEMUA asset FreeplayData satuan (bao, bei, biao, dll) ke sini sekaligus")]
    [SerializeField] private List<FreeplayData> allSingleCardData;

    [Header("Daftar Semua Marker di Hierarchy")]
    [Tooltip("Tarik semua GameObject Marker (bao, bei, dll) ke sini sekali saja")]
    [SerializeField] private List<GameObject> allMarkers;

    [Header("Settings")]
    [SerializeField] private float proximityThreshold = 15f; 

    [Header("Combo Stability Settings")]
    [SerializeField] private float comboDestructionDelay = 0.5f; 
    private float comboLoseTrackingTimer = 0f;
    private bool wasComboActiveLastFrame = false;

    private List<GameObject> cardsOnCamera = new List<GameObject>();
    private Dictionary<string, GameObject> activeSynthesisObjects = new Dictionary<string, GameObject>(); 
    private Dictionary<string, GameObject> activeSingleObjects = new Dictionary<string, GameObject>();    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        ScanActiveCards();
        ManageSingleAndComboLogic();
    }

    private void ScanActiveCards()
    {
        cardsOnCamera.Clear();
        foreach (GameObject marker in allMarkers)
        {
            if (marker == null) continue;

            ZicharaCard cardScript = marker.GetComponent<ZicharaCard>();
            if (cardScript != null && cardScript.isTracked)
            {
                cardsOnCamera.Add(marker);
            }
        }
    }

    private void ManageSingleAndComboLogic()
    {
        List<CardRecipe> validCombos = GetValidFormedRecipes();

        if (validCombos.Count > 0)
        {
            // Reset timer karena combo terdeteksi valid
            comboLoseTrackingTimer = 0f;
            wasComboActiveLastFrame = true;

            // SAPU BERSIH: Matikan stroke satuan terlebih dahulu
            ClearAllSingleHanzi();
            
            CheckAndExecuteCombos(validCombos);
            UpdateComboPositions();
        }
        else
        {
            // Jika frame sebelumnya ada combo, beri jeda toleransi sebelum dihancurkan
            if (wasComboActiveLastFrame)
            {
                comboLoseTrackingTimer += Time.deltaTime;
                if (comboLoseTrackingTimer >= comboDestructionDelay)
                {
                    wasComboActiveLastFrame = false;
                    ClearAllComboObjects();
                    CheckAndExecuteSingleHanzi();
                }
                else
                {
                    UpdateComboPositions();
                }
            }
            else
            {
                ClearAllComboObjects();
                CheckAndExecuteSingleHanzi();
            }
        }
    }

    #region LOGIKA KARTU SATUAN
    private void CheckAndExecuteSingleHanzi()
    {
        if (allSingleCardData == null || allSingleCardData.Count == 0) return;

        HashSet<string> currentActiveCardNames = new HashSet<string>();
        foreach (GameObject card in cardsOnCamera)
        {
            currentActiveCardNames.Add(card.name.ToLower());
        }

        foreach (GameObject marker in allMarkers)
        {
            if (marker == null) continue;
            string markerNameLower = marker.name.ToLower();

            Transform strokeAnimatorTransform = marker.transform.Find("HanziStrokeAnimator");
            if (strokeAnimatorTransform == null) continue;

            GameObject strokeObj = strokeAnimatorTransform.gameObject;

            if (currentActiveCardNames.Contains(markerNameLower))
            {
                if (!activeSingleObjects.ContainsKey(markerNameLower))
                {
                    strokeObj.SetActive(true);
                    activeSingleObjects.Add(markerNameLower, strokeObj);

                    foreach (FreeplayData singleData in allSingleCardData)
                    {
                        if (singleData == null || singleData.hanziRecipes == null) continue;

                        HanziRecipe singleRecipe = singleData.hanziRecipes.Find(r => 
                            r.requiredCards != null && 
                            r.requiredCards.Count == 1 && 
                            markerNameLower.Contains(r.requiredCards[0].ToLower())
                        );

                        if (singleRecipe != null && singleRecipe.successSound != null)
                        {
                            AudioSource.PlayClipAtPoint(singleRecipe.successSound, marker.transform.position);
                            break; 
                        }
                    }

                    Debug.Log($"[Freeplay] Mengaktifkan Stroke Bawaan: {marker.name}");
                }
            }
            else
            {
                if (activeSingleObjects.ContainsKey(markerNameLower))
                {
                    strokeObj.SetActive(false);
                    activeSingleObjects.Remove(markerNameLower);
                }
            }
        }
    }

    // Fungsi ini telah diperbaiki total untuk menyapu bersih seluruh stroke di Hierarchy
    private void ClearAllSingleHanzi()
    {
        // 1. Matikan objek yang tercatat di Dictionary
        foreach (var obj in activeSingleObjects.Values)
        {
            if (obj != null) obj.SetActive(false);
        }
        activeSingleObjects.Clear();

        // 2. Jaga-jaga sapu bersih seluruh list marker untuk mematikan stroke yang 'ketinggalan' akibat event Vuforia
        foreach (GameObject marker in allMarkers)
        {
            if (marker == null) continue;
            Transform strokeAnimatorTransform = marker.transform.Find("HanziStrokeAnimator");
            if (strokeAnimatorTransform != null)
            {
                strokeAnimatorTransform.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region LOGIKA KARTU COMBO
    private List<CardRecipe> GetValidFormedRecipes()
    {
        List<CardRecipe> formedRecipes = new List<CardRecipe>();
        if (allMissionsData == null) return formedRecipes;

        foreach (MissionData mission in allMissionsData)
        {
            if (mission == null) continue;
            foreach (CardRecipe recipe in mission.recipes)
            {
                if (recipe == null || recipe.resultPrefab == null) continue;

                if (IsRecipeFormed(recipe))
                {
                    formedRecipes.Add(recipe);
                }
            }
        }
        return formedRecipes;
    }

    private bool IsRecipeFormed(CardRecipe recipe)
    {
        if (recipe.requiredCards == null || recipe.requiredCards.Count < 2) return false;

        List<GameObject> foundCards = new List<GameObject>();
        foreach (string reqCardName in recipe.requiredCards)
        {
            GameObject found = cardsOnCamera.Find(card => 
                card.name.ToLower().Contains(reqCardName.ToLower())
            );

            if (found == null) return false;
            foundCards.Add(found);
        }

        Vector3 center = Vector3.zero;
        foreach (var card in foundCards) center += card.transform.position;
        center /= foundCards.Count;

        foreach (var card in foundCards)
        {
            if (Vector3.Distance(card.transform.position, center) > proximityThreshold)
                return false;
        }

        return true;
    }

    private void CheckAndExecuteCombos(List<CardRecipe> validCombos)
    {
        foreach (CardRecipe recipe in validCombos)
        {
            if (!activeSynthesisObjects.ContainsKey(recipe.recipeName))
            {
                Vector3 spawnPos = CalculateCentroid(recipe.requiredCards);
                GameObject obj = Instantiate(recipe.resultPrefab, spawnPos, Quaternion.identity);
                activeSynthesisObjects.Add(recipe.recipeName, obj);
                
                Debug.Log($"[Freeplay] Berhasil memunculkan combo: {recipe.recipeName}");
            }
        }

        List<string> combosToRemove = new List<string>();
        foreach (string spawnedName in activeSynthesisObjects.Keys)
        {
            if (!validCombos.Exists(r => r.recipeName == spawnedName))
            {
                combosToRemove.Add(spawnedName);
            }
        }

        foreach (string name in combosToRemove)
        {
            ClearSingleCombo(name);
        }
    }

    private void UpdateComboPositions()
    {
        foreach (var entry in activeSynthesisObjects)
        {
            CardRecipe recipe = FindRecipeInAllMissions(entry.Key);
            if (recipe == null || entry.Value == null) continue;

            entry.Value.transform.position = CalculateCentroid(recipe.requiredCards);

            GameObject refCard = cardsOnCamera.Find(card => 
                card.name.ToLower().Contains(recipe.requiredCards[0].ToLower())
            );
            
            if (refCard != null)
            {
                entry.Value.transform.rotation = refCard.transform.rotation;
            }
        }
    }

    private void ClearAllComboObjects()
    {
        foreach (var obj in activeSynthesisObjects.Values)
        {
            if (obj != null) Destroy(obj);
        }
        activeSynthesisObjects.Clear();
    }

    private void ClearSingleCombo(string recipeName)
    {
        if (activeSynthesisObjects.TryGetValue(recipeName, out GameObject obj))
        {
            if (obj != null) Destroy(obj);
            activeSynthesisObjects.Remove(recipeName);
        }
    }
    #endregion

    #region UTILITIES
    private Vector3 CalculateCentroid(List<string> reqCards)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (string id in reqCards)
        {
            GameObject c = cardsOnCamera.Find(card => card.name.ToLower().Contains(id.ToLower()));
            if (c != null)
            {
                sum += c.transform.position;
                count++;
            }
        }
        return count > 0 ? sum / count : Vector3.zero;
    }

    private CardRecipe FindRecipeInAllMissions(string recipeName)
    {
        foreach (MissionData m in allMissionsData)
        {
            if (m == null) continue;
            CardRecipe r = m.recipes.Find(rcp => rcp.recipeName == recipeName);
            if (r != null) return r;
        }
        return null;
    }
    #endregion
}