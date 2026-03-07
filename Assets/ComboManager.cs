using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public bool kartu1Active = false;
    public bool kartu2Active = false;
    
    [Header("Model Settings")]
    public GameObject modelFBXCombo; // Ini untuk si 'kyut'
    public GameObject modelKartu1;   // Model yang muncul kalau cuma kartu 1
    public GameObject modelKartu2;   // Model yang muncul kalau cuma kartu 2

    void Update()
    {
        // Kondisi 1: Keduanya muncul (COMBO)
        if (kartu1Active && kartu2Active) {
            if(modelFBXCombo != null) modelFBXCombo.SetActive(true);
            if(modelKartu1 != null) modelKartu1.SetActive(false);
            if(modelKartu2 != null) modelKartu2.SetActive(false);
        } 
        // Kondisi 2: Cuma kartu 1
        else if (kartu1Active) {
            if(modelFBXCombo != null) modelFBXCombo.SetActive(false);
            if(modelKartu1 != null) modelKartu1.SetActive(true);
            if(modelKartu2 != null) modelKartu2.SetActive(false);
        }
        // Kondisi 3: Cuma kartu 2
        else if (kartu2Active) {
            if(modelFBXCombo != null) modelFBXCombo.SetActive(false);
            if(modelKartu1 != null) modelKartu1.SetActive(false);
            if(modelKartu2 != null) modelKartu2.SetActive(true);
        }
        // Kondisi 4: Tidak ada kartu
        else {
            if(modelFBXCombo != null) modelFBXCombo.SetActive(false);
            if(modelKartu1 != null) modelKartu1.SetActive(false);
            if(modelKartu2 != null) modelKartu2.SetActive(false);
        }
    }

    public void SetKartu1(bool status) { kartu1Active = status; }
    public void SetKartu2(bool status) { kartu2Active = status; }
}