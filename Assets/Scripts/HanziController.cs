using UnityEngine;
using System.Collections;

public class HanziController : MonoBehaviour
{
    public LineAnimator[] strokes;       // drag Stroke1..9 here
    public float pauseBetween = 0.2f;
    public float loopDelay    = 1.5f;
    public bool  autoLoop     = true;

    void Start() => StartCoroutine(PlayAll());

    IEnumerator PlayAll()
    {
    foreach (var s in strokes)
        s.gameObject.SetActive(false);

    yield return new WaitForSeconds(0.3f);

    foreach (var s in strokes)
    {
        s.gameObject.SetActive(true);
        s.PlayStroke(); // <--- TAMBAHKAN INI

        yield return new WaitForSeconds(s.GetDuration() + pauseBetween);
    }

        if (autoLoop)
        {
            yield return new WaitForSeconds(loopDelay);
            // Restart — deactivate all then replay
            foreach (var s in strokes)
                s.gameObject.SetActive(false);
            StartCoroutine(PlayAll());
        }
    }
}