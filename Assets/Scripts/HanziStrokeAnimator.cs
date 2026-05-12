using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HanziStrokeAnimator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float strokeDuration = 0.5f;
    [SerializeField] private FreeplayData currentData;
    
    [Header("UI Target")]
    [SerializeField] private Transform uiCanvasTransform;

    private GameObject currentSpawnedHanzi;

    private void OnEnable()
    {
        // Berhenti dulu kalau ada animasi sisa yang masih jalan
        StopAllCoroutines();
        CleanUp();

        if (currentData != null) 
        {
            StartCoroutine(PlayAllRecipes());
        }
    }

    private IEnumerator PlayAllRecipes()
    {
        foreach (var recipe in currentData.hanziRecipes)
        {
            if (recipe.hanziPrefab == null) continue;

            // Cari objek yang sudah kamu taruh di Canvas (biar ga double)
            if (currentSpawnedHanzi == null)
            {
                Transform found = uiCanvasTransform.Find(recipe.hanziPrefab.name);
                if (found != null) currentSpawnedHanzi = found.gameObject;
            }

            if (currentSpawnedHanzi != null)
            {
                currentSpawnedHanzi.SetActive(true);
                
                // Ambil semua line renderer secara berurutan
                LineRenderer[] allLines = currentSpawnedHanzi.GetComponentsInChildren<LineRenderer>(true);

                // Reset semua garis jadi mati dulu
                foreach (var lr in allLines) lr.gameObject.SetActive(false);

                // Mulai gambar satu-satu
                for (int i = 0; i < allLines.Length; i++)
                {
                    allLines[i].gameObject.SetActive(true);
                    yield return StartCoroutine(AnimateSingleStroke(allLines[i]));
                    
                    // Jeda sangat singkat antar garis biar transisinya mulus
                    yield return new WaitForSeconds(0.05f); 
                }
            }
        }
    }

    private IEnumerator AnimateSingleStroke(LineRenderer lr)
{
    // 1. Ambil jumlah titik yang real saat ini
    int pointCount = lr.positionCount;
    if (pointCount < 2) yield break;

    // 2. Simpan posisi asli
    Vector3[] allPoints = new Vector3[pointCount];
    for (int i = 0; i < pointCount; i++)
    {
        allPoints[i] = lr.GetPosition(i);
    }

    // 3. Reset tampilan: semua titik ditarik ke titik awal
    for (int i = 1; i < pointCount; i++)
    {
        lr.SetPosition(i, allPoints[0]);
    }

    float segmentDuration = strokeDuration / (pointCount - 1);

    // 4. Mulai animasi per segmen
    for (int i = 0; i < pointCount - 1; i++)
    {
        float elapsed = 0f;
        Vector3 start = allPoints[i];
        Vector3 end = allPoints[i + 1];

        while (elapsed < segmentDuration)
        {
            // Proteksi jika kartu hilang saat animasi
            if (lr == null || !this.gameObject.activeInHierarchy) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / segmentDuration);
            Vector3 currentPos = Vector3.Lerp(start, end, t);

            // Update titik saat ini DAN semua titik setelahnya agar tidak melintang
            for (int j = i + 1; j < lr.positionCount; j++)
            {
                lr.SetPosition(j, currentPos);
            }

            yield return null;
        }

        // Pastikan titik terkunci di posisi akhir segmen
        if (lr != null && i + 1 < lr.positionCount)
        {
            lr.SetPosition(i + 1, end);
        }
    }
}

    private void OnDisable()
    {
        StopAllCoroutines();
        CleanUp();
    }

    private void CleanUp()
    {
        if (currentSpawnedHanzi != null)
        {
            currentSpawnedHanzi.SetActive(false);
        }
    }
}