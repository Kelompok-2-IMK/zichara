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
    private AudioSource audioSource;

    private void OnEnable()
    {
        // Inisialisasi AudioSource di sini, bukan di Awake
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

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

            // Null check sebelum play sound
            if (recipe.successSound != null && audioSource != null)
                audioSource.PlayOneShot(recipe.successSound);

            if (currentSpawnedHanzi == null)
            {
                Transform found = uiCanvasTransform.Find(recipe.hanziPrefab.name);
                if (found != null) currentSpawnedHanzi = found.gameObject;
            }

            if (currentSpawnedHanzi != null)
            {
                currentSpawnedHanzi.SetActive(true);
                
                LineRenderer[] allLines = currentSpawnedHanzi.GetComponentsInChildren<LineRenderer>(true);

                foreach (var lr in allLines) lr.gameObject.SetActive(false);

                for (int i = 0; i < allLines.Length; i++)
                {
                    allLines[i].gameObject.SetActive(true);
                    yield return StartCoroutine(AnimateSingleStroke(allLines[i]));
                    yield return new WaitForSeconds(0.05f); 
                }
            }
        }
    }

    private IEnumerator AnimateSingleStroke(LineRenderer lr)
    {
        int pointCount = lr.positionCount;
        if (pointCount < 2) yield break;

        Vector3[] allPoints = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            allPoints[i] = lr.GetPosition(i);
        }

        for (int i = 1; i < pointCount; i++)
        {
            lr.SetPosition(i, allPoints[0]);
        }

        float segmentDuration = strokeDuration / (pointCount - 1);

        for (int i = 0; i < pointCount - 1; i++)
        {
            float elapsed = 0f;
            Vector3 start = allPoints[i];
            Vector3 end = allPoints[i + 1];

            while (elapsed < segmentDuration)
            {
                if (lr == null || !this.gameObject.activeInHierarchy) yield break;

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / segmentDuration);
                Vector3 currentPos = Vector3.Lerp(start, end, t);

                for (int j = i + 1; j < lr.positionCount; j++)
                {
                    lr.SetPosition(j, currentPos);
                }

                yield return null;
            }

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