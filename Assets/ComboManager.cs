using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [Header("Status Kartu")]
    public bool kartu1Active = false;
    public bool kartu2Active = false;
    
    [Header("Referensi Objek Kartu (Tarik dari Hierarchy)")]
    public Transform kartu1Transform; 
    public Transform kartu2Transform;

    [Header("Model Settings")]
    public GameObject modelFBXCombo; // Ini untuk si 'kyut'
    public GameObject modelKartu1;   // Model 'monyet'
    public GameObject modelKartu2;   // Model 'wo'

    void Update()
    {
        UpdateLogic();
    }

    void UpdateLogic()
    {
        // 1. Kondisi COMBO: Keduanya muncul
        if (kartu1Active && kartu2Active) 
        {
            SetModelStates(true, false, false);
            FollowMiddlePoint(); // Pindahkan 'kyut' ke tengah
        } 
        // 2. Cuma kartu 1 (monyet)
        else if (kartu1Active) 
        {
            SetModelStates(false, true, false);
        }
        // 3. Cuma kartu 2 (wo)
        else if (kartu2Active) 
        {
            SetModelStates(false, false, true);
        }
        // 4. Tidak ada kartu sama sekali (Force Hide)
        else 
        {
            SetModelStates(false, false, false);
        }
    }

    // Fungsi pembantu agar kode Update tidak kepanjangan
    void SetModelStates(bool combo, bool m1, bool m2)
    {
        if(modelFBXCombo != null) modelFBXCombo.SetActive(combo);
        if(modelKartu1 != null) modelKartu1.SetActive(m1);
        if(modelKartu2 != null) modelKartu2.SetActive(m2);
    }

    // Fungsi agar si 'kyut' berada di tengah-tengah dua kartu
    void FollowMiddlePoint()
    {
        if (modelFBXCombo != null && kartu1Transform != null && kartu2Transform != null)
        {
            // Rumus posisi tengah: (A + B) / 2
            Vector3 midPoint = (kartu1Transform.position + kartu2Transform.position) / 2f;
            modelFBXCombo.transform.position = midPoint;

            // Optional: Agar kyut menghadap ke arah rata-rata rotasi kartu
            modelFBXCombo.transform.rotation = Quaternion.Slerp(kartu1Transform.rotation, kartu2Transform.rotation, 0.5f);
        }
    }

    // Fungsi pemicu dari Vuforia (Default Observer Event Handler)
    public void SetKartu1(bool status) { kartu1Active = status; }
    public void SetKartu2(bool status) { kartu2Active = status; }
}