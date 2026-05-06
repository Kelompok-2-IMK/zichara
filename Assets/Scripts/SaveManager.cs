using UnityEngine;
using System.IO;

public static class SaveManager
{
    // Lokasi aman untuk menyimpan data di memori HP/PC
    private static string saveFilePath = Application.persistentDataPath + "/ZicharaSaveData.json";

    // Fungsi untuk Menyimpan Data
    public static void SaveData(PlayerData data)
    {
        // Ubah objek C# menjadi teks JSON
        string json = JsonUtility.ToJson(data, true); 
        
        // Tulis teks tersebut ke dalam file
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Zichara Progress Tersimpan di: " + saveFilePath);
    }

    // Fungsi untuk Membaca Data
    public static PlayerData LoadData()
    {
        // Cek apakah pemain sudah punya file save (bukan pemain baru)
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            // Ubah teks JSON kembali menjadi objek C#
            PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);
            return loadedData;
        }
        else
        {
            Debug.Log("Save file tidak ditemukan. Membuat profil pemain baru.");
            // Kembalikan data kosong (level 1) jika pemain baru
            return new PlayerData(); 
        }
    }
}