using System.Collections.Generic;
using UnityEngine;

// --- STRUKTUR RESEP KARTU ---
[System.Serializable]
public class CardRecipe
{
    public string recipeName; // Misal: "Roti" atau "Sepeda"
    public List<string> requiredCards; // Misal: ["Mian", "Bao"] atau ["Zi", "Xing", "Che"]
    public GameObject resultPrefab; // Model 3D-nya
    public AudioClip recipeSuccessSound; // Suara saat berhasil buat resep
}

// Aturan Menang: Harus nemu SEMUA resep, atau SALAH SATU saja?
public enum MissionCompletionRule 
{ 
    RequireAllRecipes, // Untuk Misi 1 (Roti DAN Tas)
    RequireAnyRecipe   // Untuk Misi 2 (Sepeda ATAU Kereta) 
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
    public List<CardRecipe> recipes; // Daftar kombinasi yang bisa dibuat

    [Header("Teks Cerita (Tugas Temanmu)")]
    [TextArea(3, 5)] public string startStoryText;
    [TextArea(3, 5)] public string endStoryText;

    [Header("Sound")]
    public AudioClip missionStartSound;   // bunyi saat misi dimulai
    public AudioClip missionFinishSound;  // bunyi saat semua resep selesai
}