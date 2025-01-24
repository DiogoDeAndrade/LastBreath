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

            float s = Mathf.Lerp(sizeRange.x, sizeRange.y, oxygen / maxOxygen);
            transform.localScale = new Vector3(s, s, s);
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
    }

    public void ChangeOxygen(float delta)
    {
        oxygen = Mathf.Clamp(oxygen + delta, 0.0f, maxOxygen);
    }
}
