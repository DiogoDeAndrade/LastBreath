using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UIElements;

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
    [SerializeField, Header("Reload & Repair")]
    private bool        useScaleOnRadius = false;
    [SerializeField]
    private Transform   reloadPivot;
    [SerializeField]
    private float       reloadRadius = 50.0f;
    [SerializeField]
    private float       reloadTime = 1.0f;
    [SerializeField]
    private float       reloadOxygenDrain = 0.0f;
    [SerializeField]
    private Transform   repairPivot;
    [SerializeField]
    private float       repairRadius = 50.0f;
    [SerializeField]
    private float       repairSpeed = 25.0f;
    [SerializeField]
    private float       repairOxygenDrain = 0.0f;

    [SerializeField, Header("Visuals")]
    private SpriteRenderer  bubbleRenderer;
    [SerializeField]
    private Gradient    bubbleGradient;

    float       penaltyTimer = 0.0f;
    Submarine   player;
    float       oxygen;
    float       ammoReload;

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
                if (penaltyTimer > 0.0f)
                {
                    penaltyTimer -= Time.deltaTime;
                }
                if (penaltyTimer <= 0.0f)
                {
                    player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                    player.playerId = playerId;
                    player.name = $"Player {playerId}";
                    penaltyTimer = penaltyDuration;
                }
            }
            else
            {
                if ((reloadPivot) && (player.ammo < player.maxAmmo))
                {
                    float d = Vector3.Distance(reloadPivot.position, player.transform.position);
                    if (d < AdjustRadius(reloadRadius))
                    {
                        ammoReload += Time.deltaTime;

                        ChangeOxygen(-reloadOxygenDrain * Time.deltaTime);
                        if (ammoReload > reloadTime)
                        {
                            player.AddAmmo(1);
                            ammoReload -= reloadTime;
                        }
                    }
                }
                var healthSystem = player.GetComponent<HealthSystem>();
                if ((repairPivot) && (healthSystem.health < healthSystem.maxHealth))
                {
                    float d = Vector3.Distance(repairPivot.position, player.transform.position);
                    if (d < AdjustRadius(repairRadius))
                    {
                        ChangeOxygen(-repairOxygenDrain * Time.deltaTime);
                        healthSystem.Heal(Time.deltaTime * repairSpeed, false);
                        if (ammoReload > reloadTime)
                        {
                            player.AddAmmo(1);
                            ammoReload -= reloadTime;
                        }
                    }
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
        //bubbleRenderer.color = Color.white;
        bubbleRenderer.FadeTo(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.25f).Done(() => { Debug.Log("Level Over"); }).EaseFunction(Ease.Sqrt);
    }

    float AdjustRadius(float r)
    {
        if (useScaleOnRadius) return r * transform.localScale.x;

        return r;
    }

    private void OnDrawGizmosSelected()
    {
        if (reloadPivot)
        {
            float r = AdjustRadius(reloadRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(reloadPivot.position, r);
        }
        if (repairPivot)
        {
            float r = AdjustRadius(repairRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(repairPivot.position, r);
        }
    }
}
