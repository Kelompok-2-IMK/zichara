using UnityEngine;
using Vuforia;
using System.Collections;

[RequireComponent(typeof(ObserverBehaviour))]
[RequireComponent(typeof(AudioSource))]  // ← tambah ini
public class ZicharaCard : MonoBehaviour
{
    public string cardID;
    public AudioClip scanSound;
    
    [Tooltip("Waktu toleransi (detik) jika kartu tiba-tiba hilang fokus di HP")]
    public float lostDelay = 0.75f; 

    private ObserverBehaviour mObserverBehaviour;
    private AudioSource audioSource;  // ← tambah ini
    private Coroutine lostCoroutine;
    private bool hasPlayedSound = false;  // ← biar sound gak spam tiap frame

    void Start()
    {
        audioSource = GetComponent<AudioSource>();  // ← tambah ini
        audioSource.playOnAwake = false;            // ← matiin auto-play

        mObserverBehaviour = GetComponent<ObserverBehaviour>();
        if (mObserverBehaviour)
        {
            mObserverBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;

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
            hasPlayedSound = false;  // ← reset biar bisa bunyi lagi saat scan ulang

            if (gameObject.activeInHierarchy)
            {
                if (lostCoroutine != null) StopCoroutine(lostCoroutine);
                lostCoroutine = StartCoroutine(DelayedRemove());
            }
        }
    }

    private void ReportArrival()
    {
        // Play sound sekali saat pertama kali terdeteksi
        if (!hasPlayedSound && scanSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scanSound);
            hasPlayedSound = true;
        }

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
        if (CardSynthesisManager.Instance != null)
        {
            CardSynthesisManager.Instance.RemoveActiveCard(this);
        }

        if (mObserverBehaviour) 
            mObserverBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }
}