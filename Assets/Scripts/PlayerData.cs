using System.Collections.Generic;
using UnityEngine;

[System.Serializable] 
public class PlayerData
{
    // Level default saat pertama kali main adalah 1
    public int lastUnlockedLevel = 1; 
    
    // Menyimpan ID misi yang sudah beres (misal: "L1_Misi1_Bekal")
    public List<string> completedMissions = new List<string>(); 
    
    // Menyimpan ID barang yang sudah di-scan untuk halaman "Legends"
    public List<string> collectedItems = new List<string>(); 
}