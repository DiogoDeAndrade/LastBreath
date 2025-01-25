using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityUI : MonoBehaviour
{
    [SerializeField] private City               city;
    [SerializeField] private Image              image;
    [SerializeField] private TextMeshProUGUI    text;

    CanvasGroup canvasGroup;
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
    }

    void Update()
    {
        if (city.isPlayerDead)
        {
            canvasGroup.FadeIn(0.5f);

            image.gameObject.SetActive(false);
            text.color = Color.red;

            float totalTime = city.timeToRespawn;
            int seconds = 1 + Mathf.FloorToInt(totalTime) % 60;
            text.text = $"{seconds.ToString("D2")}s";

            return;
        }

        var type = city.requestItem;

        if (type == null)
        {
            canvasGroup.FadeOut(0.5f);
        }
        else
        {
            var quantity = city.requestCount;

            canvasGroup.FadeIn(0.5f);
            image.gameObject.SetActive(true);
            image.sprite = type.nodeImage;
            text.color = type.displayColor;
            text.text = $"x{quantity}";
        }
    }
}
