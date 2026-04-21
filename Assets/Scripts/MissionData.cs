using System.Collections.Generic;
using UnityEngine;

// Baris ini membuat kita bisa klik kanan di Unity -> Create -> Zichara -> Mission Data
[CreateAssetMenu(fileName = "NewMission", menuName = "Zichara/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Identitas Misi")]
    public string missionID; // Misal: "L1_Bekal"
    public string missionTitle; // Misal: "Misi 1: Cari Bekal"
    
    [Header("Logika AR")]
    // Daftar ID ImageTarget Vuforia yang harus discan berbarengan (Misal: "Mian", "Bao")
    public List<string> requiredCards; 
    public GameObject resultPrefab; // Model 3D Roti yang akan muncul

    [Header("Teks Cerita (Dinamic Popup)")]
    [TextArea(3, 5)]
    public string startStoryText; // Muncul saat misi baru dimulai
    [TextArea(3, 5)]
    public string endStoryText;   // Muncul saat misi berhasil
}