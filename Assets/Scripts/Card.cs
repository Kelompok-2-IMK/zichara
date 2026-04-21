using UnityEngine;
using Vuforia; // Wajib untuk Vuforia 10+

[RequireComponent(typeof(ObserverBehaviour))]
public class Card : MonoBehaviour
{
    [Header("Identitas Kartu")]
    public string cardID; // Contoh: "Mian" atau "Bao"

    private ObserverBehaviour mObserverBehaviour;

    void Start()
    {
        mObserverBehaviour = GetComponent<ObserverBehaviour>();
        // Daftarkan fungsi untuk memantau status kartu (Terlihat / Hilang)
        if (mObserverBehaviour)
        {
            mObserverBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        // Jika kartu terlihat (Tracked atau Extended Tracked)
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            CardSynthesisManager.Instance.AddActiveCard(this);
        }
        else // Jika kartu hilang dari kamera
        {
            CardSynthesisManager.Instance.RemoveActiveCard(this);
        }
    }

    private void OnDestroy()
    {
        if (mObserverBehaviour)
            mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }
}