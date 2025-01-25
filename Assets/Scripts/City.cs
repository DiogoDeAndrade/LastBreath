using NaughtyAttributes;
using UnityEngine;

public class City : MonoBehaviour
{
    [SerializeField] 
    private int         playerId;
    [SerializeField, Header("Player Spawn")]
    private Submarine   playerPrefab;
    [SerializeField] 
    private Transform   spawnPoint;
    [SerializeField] 
    private float       penaltyDuration = 10.0f;
    [SerializeField, Header("Progression")]
    private Vector2     sizeRange = Vector2.one;
    [SerializeField]
    private float       maxOxygen = 1000;
    [SerializeField]
    private float       startOxygen = 500;
    [SerializeField]
    private float       oxygenLossPerSecond = 10;
    [SerializeField, Header("Visuals")]
    private SpriteRenderer  bubbleRenderer;
    [SerializeField]
    private Gradient    bubbleGradient;

    float       penaltyTimer = 0.0f;
    Submarine   player;
    float       oxygen;

    void Start()
    {
        oxygen = startOxygen;
    }

    void Update()
    {
        if (oxygen > 0)
        {
            ChangeOxygen(-oxygenLossPerSecond * Time.deltaTime);
            if (oxygen <= 0.0f)
            {
                PopBubble();
            }

            float t = oxygen / maxOxygen;
            float s = Mathf.Lerp(sizeRange.x, sizeRange.y, t);
            transform.localScale = new Vector3(s, s, s);

            bubbleRenderer.color = bubbleGradient.Evaluate(t);

            if (player == null)
            {
                if (penaltyTimer > 0.0f) penaltyTimer -= Time.deltaTime;
                if (penaltyTimer <= 0.0f)
                {
                    player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                    player.playerId = playerId;
                    player.name = $"Player {playerId}";
                    penaltyTimer = penaltyDuration;
                }
            }
        }
        else
        {

        }
    }

    public void ChangeOxygen(float delta)
    {
        oxygen = Mathf.Clamp(oxygen + delta, 0.0f, maxOxygen);
    }

    [Button("Pop bubble")]
    public void PopBubble()
    {
        float s = transform.localScale.x;
        transform.ScaleTo(new Vector3(s * 1.1f, s * 1.1f, s * 1.1f), 0.25f).EaseFunction(Ease.Sqrt);
        bubbleRenderer.color = Color.white;
        bubbleRenderer.FadeTo(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.25f).Done(() => { Debug.Log("Level Over"); }).EaseFunction(Ease.Sqrt);
    }
}
