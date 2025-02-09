using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PhaseDisplay : MonoBehaviour
{
    [SerializeField] private int phase;
    [SerializeField] private float displayDelay = 0;
    [SerializeField] private float waitTime = 4.0f;

    bool display = false;
    CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
    }

    void Update()
    {
        if (!display)
        {
            if (LevelManager.phase == phase)
            {
                display = true;

                StartCoroutine(DisplayCR());
            }
        }
    }

    IEnumerator DisplayCR()
    {
        yield return new WaitForSeconds(displayDelay);

        transform.localScale = Vector3.zero;
        transform.ScaleTo(Vector3.one, 0.25f).EaseFunction(Ease.Sqrt);

        yield return new WaitForTween(canvasGroup.FadeIn(1.0f));

        yield return new WaitForSeconds(waitTime);

        canvasGroup.FadeIn(0.5f);
        transform.Move(Vector3.up * 200.0f, 0.25f).EaseFunction(Ease.Sqr);

        yield return new WaitForTween(canvasGroup.FadeOut(0.25f));

        Destroy(gameObject);
    }
}
