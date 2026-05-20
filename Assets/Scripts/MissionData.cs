using System.Collections.Generic;
using UnityEngine;

// --- STRUKTUR RESEP KARTU ---
[System.Serializable]
public class CardRecipe
{
    public string recipeName; 
    public List<string> requiredCards; 
    public GameObject resultPrefab; 
    public AudioClip recipeSuccessSound; 
    
    // --- FITUR QOL: TEKS PINYIN ---
    [Tooltip("Teks Pinyin yang akan melayang di atas 3D")]
    public string pinyinText; 
}

public enum MissionCompletionRule 
{ 
    RequireAllRecipes, 
    RequireAnyRecipe   
}

[CreateAssetMenu(fileName = "NewMission", menuName = "Zichara/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Identitas Misi")]
    public string missionID;
    public string missionTitle;

    [Header("Logika Multi-Resep")]
    [Tooltip("Pilih RequireAll untuk Misi 1, pilih RequireAny untuk Misi 2")]
    public MissionCompletionRule completionRule;
    public List<CardRecipe> recipes; 

    [Header("Teks Cerita (Tugas Temanmu)")]
    [TextArea(3, 5)] public string startStoryText;
    [TextArea(3, 5)] public string endStoryText;

    [Header("Sound")]
    public AudioClip missionStartSound;   
    public AudioClip missionFinishSound;  
}