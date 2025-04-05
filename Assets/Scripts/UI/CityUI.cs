using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UC;

public class CityUI : MonoBehaviour
{
    [SerializeField] private City               city;
    [SerializeField] private Image              image;
    [SerializeField] private TextMeshProUGUI    text;
    [SerializeField] private Sprite             anyResourceSprite;

    [SerializeField, Header("Broadcast Animation")]
    private Animator        broadcastAnimator;  
    [SerializeField]
    private float           broadcastSleepTime = 2.0f;

    CanvasGroup     canvasGroup;
    float           broadcastTimer;
    ResourceData    prevRequest;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
        broadcastTimer = broadcastSleepTime;
    }

    void Update()
    {
        if (city.isDead)
        {
            if (!city.isReviving)
            {
                if ((city.playerOnReviveArea) && (city.itemsRequiredToRevive > 0))
                {
                    canvasGroup.FadeIn(0.15f);

                    image.gameObject.SetActive(true);
                    image.sprite = anyResourceSprite;
                    text.color = Color.red;

                    text.text = $"x{city.itemsRequiredToRevive}";

                    broadcastAnimator.speed = 0.5f;
                    broadcastTimer -= Time.deltaTime * 0.5f;
                    if (broadcastTimer <= 0.0f)
                    {
                        broadcastAnimator.SetTrigger("Reset");
                        broadcastTimer = broadcastSleepTime;
                    }
                }
                else
                {
                    canvasGroup.FadeOut(0.15f);
                }
            }
            else
            {
                var remainingTime = city.remainingTimeToRevive;

                canvasGroup.FadeIn(0.15f);

                image.gameObject.SetActive(false);
                text.color = new Color(0.1f, 0.7f, 0.1f, 1.0f);

                int seconds = Mathf.FloorToInt(remainingTime) % 60;
                text.text = $"{seconds.ToString("D2")}s";

                broadcastAnimator.speed = 1.0f;
                broadcastTimer -= Time.deltaTime;
                if (broadcastTimer <= 0.0f)
                {
                    broadcastAnimator.SetTrigger("Reset");
                    broadcastTimer = broadcastSleepTime;
                }
            }
            return;
        }
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

            prevRequest = null; 
        }
        else
        {
            var quantity = city.requestCount;

            canvasGroup.FadeIn(0.5f);
            image.gameObject.SetActive(true);
            image.sprite = type.nodeImage;
            text.color = type.displayColor;
            text.text = $"x{quantity}";

            // Animate broadcast
            broadcastAnimator.speed = 1.0f;
            broadcastTimer -= Time.deltaTime;
            if ((broadcastTimer <= 0.0f) || (prevRequest != type))
            {
                broadcastAnimator.SetTrigger("Reset");
                broadcastTimer = broadcastSleepTime;
            }

            prevRequest = type;
        }
    }
}
