using UnityEngine;
using Vuforia;
using System.Collections;

[RequireComponent(typeof(ObserverBehaviour))]
public class ZicharaCard : MonoBehaviour
{
    public string cardID;
    
    [Tooltip("Waktu toleransi (detik) jika kartu tiba-tiba hilang fokus di HP")]
    public float lostDelay = 0.75f; 

    private ObserverBehaviour mObserverBehaviour;
    private Coroutine lostCoroutine;

    void Start()
    {
        mObserverBehaviour = GetComponent<ObserverBehaviour>();
        if (mObserverBehaviour)
        {
            mObserverBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;

            // --- TAMBAHAN PENTING ---
            // Cek status saat ini juga. Kadang pas Start, kartu sudah terdeteksi
            if (mObserverBehaviour.TargetStatus.Status == Status.TRACKED || 
                mObserverBehaviour.TargetStatus.Status == Status.EXTENDED_TRACKED)
            {
                ReportArrival();
            }
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            if (lostCoroutine != null) 
            {
                StopCoroutine(lostCoroutine);
                lostCoroutine = null;
            }
            ReportArrival();
        }
        else 
        {
            if (gameObject.activeInHierarchy)
            {
                if (lostCoroutine != null) StopCoroutine(lostCoroutine);
                lostCoroutine = StartCoroutine(DelayedRemove());
            }
        }
    }

    // Fungsi helper supaya lebih rapi dan aman dari NullReference
    private void ReportArrival()
    {
        if (CardSynthesisManager.Instance != null)
        {
            CardSynthesisManager.Instance.AddActiveCard(this);
        }
    }

    private IEnumerator DelayedRemove()
    {
        yield return new WaitForSeconds(lostDelay);
        if (CardSynthesisManager.Instance != null)
        {
            CardSynthesisManager.Instance.RemoveActiveCard(this);
        }
        lostCoroutine = null;
    }

    private void OnDestroy()
    {
        // Pastikan lapor pergi saat object dihancurkan agar tidak nyangkut di list Manager
        if (CardSynthesisManager.Instance != null)
        {
            CardSynthesisManager.Instance.RemoveActiveCard(this);
        }

        if (mObserverBehaviour) 
            mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }
}