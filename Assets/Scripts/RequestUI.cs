using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RequestUI : MonoBehaviour
{
    [SerializeField] private Image              image;
    [SerializeField] private TextMeshProUGUI    text;

    CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
    }

    public void UpdateUI(ResourceData type, int quantity)
    {
        if (type == null)
        {
            canvasGroup.FadeOut(0.5f);
        }
        else
        {
            canvasGroup.FadeIn(0.5f);
            image.sprite = type.nodeImage;
            text.color = type.displayColor;
            text.text = $"x{quantity}";
        }
    }
}
