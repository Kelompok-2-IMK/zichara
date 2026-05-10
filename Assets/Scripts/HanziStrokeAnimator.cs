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
        // Pastikan garis punya minimal 2 titik (A ke B)
        if (lr.positionCount < 2) yield break;

        Vector3 startPos = lr.GetPosition(0);
        Vector3 finalTarget = lr.GetPosition(1);

        float t = 0;
        while (t < 1.0f)
        {
            // Jika tiba-tiba kartu hilang (objek mati), langsung stop coroutine ini
            if (!this.gameObject.activeInHierarchy) yield break;

            t += Time.deltaTime / strokeDuration;
            lr.SetPosition(1, Vector3.Lerp(startPos, finalTarget, t));
            yield return null;
        }
        lr.SetPosition(1, finalTarget);
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