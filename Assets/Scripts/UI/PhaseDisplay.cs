using System.Collections;
using TMPro;
using UnityEngine;
using UC;

public class PhaseDisplay : MonoBehaviour
{
    [SerializeField] private float displayDelay = 0;
    [SerializeField] private float waitTime = 4.0f;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;

        // Get current phase
        var currentPhase = LevelManager.phase;

        if (!string.IsNullOrEmpty(currentPhase.title))
        {
            titleText.text = currentPhase.title;
            titleText.color = currentPhase.titleColor;
        }
        else
        {
            titleText.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(currentPhase.subtitle))
        {
            subtitleText.text = currentPhase.subtitle;
            subtitleText.color = currentPhase.subtitleColor;
        }
        else
        {
            subtitleText.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(currentPhase.description))
        {
            descriptionText.text = currentPhase.description;
            descriptionText.color = currentPhase.descriptionColor;
        }
        else
        {
            descriptionText.gameObject.SetActive(false);
        }

        StartCoroutine(DisplayCR());
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
