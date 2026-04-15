using UnityEngine;
using System.Collections; // Wajib ada untuk Coroutine

public class ComboManager : MonoBehaviour
{
    [Header("Status Kartu")]
    public bool kartu1Active = false;
    public bool kartu2Active = false;
    
    [Header("Settings")]
    public float loseDelay = 0.5f; // Waktu tunggu sebelum status jadi false

    [Header("Referensi Objek")]
    public Transform kartu1Transform; 
    public Transform kartu2Transform;
    public GameObject modelFBXCombo; 
    public GameObject modelKartu1;   
    public GameObject modelKartu2;

    private Coroutine delayK1;
    private Coroutine delayK2;

    void Update()
    {
        UpdateLogic();
    }

    void UpdateLogic()
    {
        if (kartu1Active && kartu2Active) 
        {
            SetModelStates(true, false, false);
            FollowMiddlePoint();
        } 
        else if (kartu1Active) 
        {
            SetModelStates(false, true, false);
        }
        else if (kartu2Active) 
        {
            SetModelStates(false, false, true);
        }
        else 
        {
            SetModelStates(false, false, false);
        }
    }

    void SetModelStates(bool combo, bool m1, bool m2)
    {
        if(modelFBXCombo != null) modelFBXCombo.SetActive(combo);
        if(modelKartu1 != null) modelKartu1.SetActive(m1);
        if(modelKartu2 != null) modelKartu2.SetActive(m2);
    }

    // --- FUNGSI UNTUK VUFORIA ---

    public void SetKartu1(bool status) 
    {
        if (status) // Jika Terdeteksi (True)
        {
            if (delayK1 != null) StopCoroutine(delayK1);
            kartu1Active = true;
        }
        else // Jika Hilang (False)
        {
            delayK1 = StartCoroutine(WaitToLoseK1());
        }
    }

    public void SetKartu2(bool status) 
    {
        if (status) 
        {
            if (delayK2 != null) StopCoroutine(delayK2);
            kartu2Active = true;
        }
        else 
        {
            delayK2 = StartCoroutine(WaitToLoseK2());
        }
    }

    IEnumerator WaitToLoseK1()
    {
        yield return new WaitForSeconds(loseDelay);
        kartu1Active = false;
    }

    IEnumerator WaitToLoseK2()
    {
        yield return new WaitForSeconds(loseDelay);
        kartu2Active = false;
    }

    void FollowMiddlePoint()
    {
        if (modelFBXCombo != null && kartu1Transform != null && kartu2Transform != null)
        {
            Vector3 midPoint = (kartu1Transform.position + kartu2Transform.position) / 2f;
            modelFBXCombo.transform.position = midPoint;
            modelFBXCombo.transform.rotation = Quaternion.Slerp(kartu1Transform.rotation, kartu2Transform.rotation, 0.5f);
        }
    }
}