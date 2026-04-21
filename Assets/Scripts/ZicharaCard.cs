using UnityEngine;
using Vuforia;
using System.Collections; // Wajib untuk Coroutine

[RequireComponent(typeof(ObserverBehaviour))]
public class ZicharaCard : MonoBehaviour
{
    public string cardID;
    
    [Tooltip("Waktu toleransi (detik) jika kartu tiba-tiba hilang fokus di HP")]
    public float lostDelay = 0.75f; 

    private ObserverBehaviour mObserverBehaviour;
    private Coroutine lostCoroutine; // Menyimpan timer

    void Start()
    {
        mObserverBehaviour = GetComponent<ObserverBehaviour>();
        if (mObserverBehaviour)
            mObserverBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        // Jika kartu TERLIHAT
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            // Batalkan proses penghapusan jika kartu kembali terlihat cepat
            if (lostCoroutine != null) 
            {
                StopCoroutine(lostCoroutine);
                lostCoroutine = null;
            }
            CardSynthesisManager.Instance.AddActiveCard(this);
        }
        else // Jika kartu HILANG
        {
            // Jangan langsung dihapus, tunggu sebentar (Anti-Flicker)
            if (gameObject.activeInHierarchy)
                lostCoroutine = StartCoroutine(DelayedRemove());
        }
    }

    private IEnumerator DelayedRemove()
    {
        yield return new WaitForSeconds(lostDelay);
        CardSynthesisManager.Instance.RemoveActiveCard(this);
    }

    private void OnDestroy()
    {
        if (mObserverBehaviour) mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }
}