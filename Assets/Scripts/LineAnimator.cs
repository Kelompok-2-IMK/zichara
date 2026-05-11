using UnityEngine;
using System.Collections;

public class LineAnimator : MonoBehaviour
{
    [SerializeField] private float animationDuration = 0.5f;

    private LineRenderer lineRenderer;
    private Vector3[]    linePoints;
    private int          pointCount;

    public float GetDuration() => animationDuration;

    // Ganti private void Start() jadi ini:
    public void PlayStroke()
    {
        // Pastikan kita ambil referensi dulu kalau belum ada
        if (lineRenderer == null) {
            lineRenderer = GetComponent<LineRenderer>();
            pointCount   = lineRenderer.positionCount;
            linePoints   = new Vector3[pointCount];

            for (int i = 0; i < pointCount; i++)
                linePoints[i] = lineRenderer.GetPosition(i);
        }

        StopAllCoroutines(); // Biar gak tumpang tindih kalau loop
        StartCoroutine(AnimateLine());
    }

    private IEnumerator AnimateLine()
    {
        // Collapse all points to the start
        for (int j = 1; j < pointCount; j++)
            lineRenderer.SetPosition(j, linePoints[0]);

        float segmentDuration = animationDuration / (pointCount - 1);

        for (int i = 0; i < pointCount - 1; i++)
        {
            float   elapsed  = 0f;
            Vector3 startPos = linePoints[i];
            Vector3 endPos   = linePoints[i + 1];

            while (elapsed < segmentDuration)
            {
                elapsed += Time.deltaTime;
                float   t   = Mathf.Clamp01(elapsed / segmentDuration);
                Vector3 tip = Vector3.Lerp(startPos, endPos, t);

                for (int j = i + 1; j < pointCount; j++)
                    lineRenderer.SetPosition(j, tip);

                yield return null;
            }

            lineRenderer.SetPosition(i + 1, endPos);
        }
    }
}