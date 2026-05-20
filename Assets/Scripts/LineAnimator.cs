using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineSequenceAnimator : MonoBehaviour
{
    [SerializeField] private float animationDuration = 1f; // Durasi per garis
    
    // Menggunakan Array agar bisa memasukkan banyak LineRenderer di Inspector
    public LineRenderer[] lines; 

    private void Start()
    {
        // Pastikan semua line dalam keadaan mati dan posisi awal benar saat mulai
        foreach (LineRenderer lr in lines)
        {
            if (lr != null)
            {
                lr.gameObject.SetActive(false);
            }
        }

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // Melakukan looping untuk setiap garis yang ada di dalam array 'lines'
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null)
            {
                lines[i].gameObject.SetActive(true); // Aktifkan garis saat ini
                yield return StartCoroutine(AnimateSingleLine(lines[i]));
            }
        }
    }

    private IEnumerator AnimateSingleLine(LineRenderer lr)
    {
        // Ambil posisi awal (index 0) dan posisi target akhir (index 1)
        Vector3 startPos = lr.GetPosition(0);
        Vector3 finalTarget = lr.GetPosition(1);

        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime / animationDuration;
            
            // Lerp posisi index 1 dari start ke target
            lr.SetPosition(1, Vector3.Lerp(startPos, finalTarget, t));
            yield return null;
        }
        
        // Pastikan posisi akhirnya presisi
        lr.SetPosition(1, finalTarget);
    }
}