using UnityEngine;
using System.Collections;

public class LineSequenceAnimator : MonoBehaviour
{
    [SerializeField] private float animationDuration = 2f;
    public LineRenderer line1;
    public LineRenderer line2;

    private void Start()
    {
        // Di awal, pastikan semua line mati dulu
        if (line1 != null) line1.gameObject.SetActive(false);
        if (line2 != null) line2.gameObject.SetActive(false);

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // 1. Munculkan dan jalankan Line 1
        if (line1 != null)
        {
            line1.gameObject.SetActive(true); // NYALAKAN LINE 1
            yield return StartCoroutine(AnimateSingleLine(line1));
        }

        // 2. Munculkan dan jalankan Line 2 SETELAH Line 1 selesai
        if (line2 != null)
        {
            line2.gameObject.SetActive(true); // NYALAKAN LINE 2
            yield return StartCoroutine(AnimateSingleLine(line2));
        }
    }

    private IEnumerator AnimateSingleLine(LineRenderer lr)
    {
        Vector3 startPos = lr.GetPosition(0);
        Vector3 finalTarget = lr.GetPosition(1);

        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime / animationDuration;
            lr.SetPosition(1, Vector3.Lerp(startPos, finalTarget, t));
            yield return null;
        }
        lr.SetPosition(1, finalTarget);
    }
}