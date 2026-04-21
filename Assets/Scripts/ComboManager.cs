using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public GameObject animasiTelepon; // Drag model telepon.fbx ke sini
    
    private bool isKartu1Detected = false;
    private bool isKartu2Detected = false;

    // Dipanggil saat Kartu 1 ditemukan
    public void SetKartu1(bool status)
    {
        isKartu1Detected = status;
        CheckCombo();
    }

    // Dipanggil saat Kartu 2 ditemukan
    public void SetKartu2(bool status)
    {
        isKartu2Detected = status;
        CheckCombo();
    }

    void CheckCombo()
    {
        // Jika dua-duanya TRUE, nyalakan animasi
        if (isKartu1Detected && isKartu2Detected)
        {
            animasiTelepon.SetActive(true);
            // Kalau mau langsung play animasi tertentu:
            // animasiTelepon.GetComponent<Animator>().Play("NamaAnimasiKamu");
        }
        else
        {
            // Jika salah satu hilang, matikan lagi (opsional)
            animasiTelepon.SetActive(false);
        }
    }
}