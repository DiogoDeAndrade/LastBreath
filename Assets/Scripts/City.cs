using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class City : MonoBehaviour
{
    [SerializeField] 
    public int          playerId;
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
    [SerializeField]
    private Transform   gatherPivot;
    [SerializeField]
    private float       gatherRadius = 50.0f;
    [SerializeField]
    private Transform   revivePivot;
    [SerializeField]
    private float       reviveRadius= 50.0f;
    [SerializeField]
    private float       reviveTime = 10.0f;
    [SerializeField]
    private float       oxygenPerResource = 50.0f;
    [SerializeField, Header("Audio")]
    private AudioClip   reloadSnd;
    [SerializeField]
    private AudioClip   requestSnd;
    [SerializeField]
    private AudioClip   respawnSnd;
    [SerializeField]
    private AudioClip   airDropSnd;
    [SerializeField]
    private AudioClip   requestSuccessSnd;
    [SerializeField]
    private AudioClip   breakerSnd;
    [SerializeField]
    private AudioClip   invBreakerSnd;
    [SerializeField, Header("Visuals")]
    private SpriteRenderer  bubbleRenderer;
    [SerializeField]
    private Gradient        bubbleGradient;
    [SerializeField]
    private SpriteRenderer  cityRenderer;
    [SerializeField]
    private Sprite          deadCitySprite;
    [SerializeField]
    private CityUI      requestUI;

    float           penaltyTimer = 0.0f;
    Submarine       player;
    float           oxygen;
    float           ammoReload;
    float           reviveTimer;
    ResourceData    requestedItem;
    int             requestedQuantity;
    float           timeOfNewRequest;
    Material        cityMaterial;
    Sprite          liveCitySprite;
    float           cityLightsBlinkTimer;
    bool            _isReviving;
    bool            firstSpawn = true;

    public bool isPlayerDead => player == null;
    public float timeToRespawn => penaltyTimer;
    public bool isDead => oxygen <= 0.0f;
    public float remainingTimeToRevive => (reviveTime > 0) ? (reviveTime - reviveTimer) : (0);
    public bool isReviving => _isReviving;

    public float oxygenPercentage => oxygen / maxOxygen;
    public float oxygenCount => oxygen;

    public ResourceData requestItem => requestedItem;
    public int          requestCount => requestedQuantity;

    void Start()
    {
        oxygen = startOxygen;
        timeOfNewRequest = 5.0f;
        cityMaterial = new Material(cityRenderer.material);
        cityRenderer.material = cityMaterial;
        cityMaterial.SetColor("_EmissiveColor", new Color(3.0f, 3.0f, 3.0f, 1.0f));
        liveCitySprite = cityRenderer.sprite;
    }

    void Update()
    {
        if (oxygen > 0)
        {
            ChangeOxygen(-oxygenLossPerSecond * Time.deltaTime);

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

                    if (firstSpawn)
                    {
                        firstSpawn = false;
                    }
                    else
                    {
                        if (respawnSnd) SoundManager.PlaySound(SoundType.PrimaryFX, respawnSnd, 1.0f, 1.0f);
                    }
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

                            if (reloadSnd) SoundManager.PlaySound(SoundType.PrimaryFX, reloadSnd, 1.0f, 1.0f);
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
                if ((gatherPivot) && (player.item != null))
                {
                    float d = Vector3.Distance(gatherPivot.position, player.transform.position);
                    if (d < AdjustRadius(gatherRadius))
                    {
                        float oxygenCount = 0.0f;
                        if (player.item == requestedItem)
                        {
                            if (player.itemCount >= requestedQuantity)
                            {
                                oxygenCount = player.item.valueMultiplier * oxygenPerResource * (requestedQuantity * 2.0f + (player.itemCount - requestedQuantity));

                                if (requestSuccessSnd) SoundManager.PlaySound(SoundType.PrimaryFX, requestSuccessSnd, 0.5f);

                                CancelAllRequests();
                            }
                        }
                        else
                        {
                            oxygenCount = oxygenPerResource * player.itemCount * player.item.valueMultiplier;

                            if (airDropSnd) SoundManager.PlaySound(SoundType.PrimaryFX, airDropSnd);
                        }
                        if (oxygenCount > 0.0f)
                        {
                            ChangeOxygen(oxygenCount);                            

                            player.DropAll(true);
                        }
                    }
                }
            }

            if (timeOfNewRequest > 0)
            {
                timeOfNewRequest -= Time.deltaTime;
                if (timeOfNewRequest <= 0.0f)
                {
                    NewRequest();
                }
            }

            if (oxygen <= 0.0f)
            {
                PopBubble();

                if (breakerSnd) SoundManager.PlaySound(SoundType.PrimaryFX, breakerSnd);
            }
            else
            {
                reviveTimer = 0;
            }

            UpdateBubbleGfx();
        }
        else
        {
            _isReviving = false;

            // City is dead, let's get it back to life!
            if ((revivePivot) && (reviveTime > 0) && (player))
            {
                float d = Vector3.Distance(revivePivot.position, player.transform.position);
                if (d < AdjustRadius(reviveRadius))
                {
                    _isReviving = true;

                    reviveTimer += Time.deltaTime;
                    if (reviveTimer > reviveTime)
                    {
                        // Bring it back to life!
                        oxygen = startOxygen;
                        if (airDropSnd) SoundManager.PlaySound(SoundType.PrimaryFX, airDropSnd);
                        if (invBreakerSnd) SoundManager.PlaySound(SoundType.PrimaryFX, invBreakerSnd);
                    }
                    else
                    {
                        cityLightsBlinkTimer -= Time.deltaTime;
                        if (cityLightsBlinkTimer < 0.0f)
                        {
                            if (Random.Range(0, 100) < 75)
                            {
                                cityRenderer.sprite = deadCitySprite;
                            }
                            else
                            {
                                cityRenderer.sprite = liveCitySprite;
                            }
                            cityLightsBlinkTimer = Random.Range(0.25f, 0.75f);
                        }
                    }
                }
                else
                {
                    cityRenderer.sprite = deadCitySprite;
                }
            }
            else
            {
                cityRenderer.sprite = deadCitySprite;
            }
        }
    }

    public void ChangeOxygen(float delta)
    {
        oxygen = Mathf.Clamp(oxygen + delta, 0.0f, maxOxygen);
    }

    [Button("Pop bubble")]
    public void PopBubble()
    {
        float s = bubbleRenderer.transform.localScale.x;
        bubbleRenderer.transform.ScaleTo(new Vector3(s * 2.1f, s * 2.1f, s * 2.1f), 0.25f).EaseFunction(Ease.Sqrt);
        bubbleRenderer.FadeTo(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.25f).EaseFunction(Ease.Sqrt);
    }

    float AdjustRadius(float r)
    {
        if (useScaleOnRadius) return r * bubbleRenderer.transform.localScale.x;

        return r;
    }

    void CancelAllRequests()
    {
        if (LevelManager.shouldCancelAllRequests)
        {
            var cities = FindObjectsByType<City>(FindObjectsSortMode.None);
            foreach (var city in cities)
            {
                city.CancelRequest();
            }
        }
        else
        {
            CancelRequest();
        }
    }

    void CancelRequest()
    {
        requestedItem = null;
        requestedQuantity = 0;
        timeOfNewRequest = 4.0f;
    }

    void NewRequest()
    {
        var resources = FindObjectsByType<Resource>(FindObjectsSortMode.InstanceID);
        Dictionary<ResourceData, int> resourceTypeCount = new();
        foreach (var resource in resources)
        {
            if (resourceTypeCount.ContainsKey(resource.data))
            {
                resourceTypeCount[resource.data]++;
            }
            else
            {
                resourceTypeCount[resource.data] = 1;
            }
        }

        if (!LevelManager.shouldCompeteForResources)
        {
            // Exclude all resources that have less than 2 available
            List<ResourceData> toRemove = new();
            foreach ((var key, var count) in resourceTypeCount)
            {
                if (count < 2) toRemove.Add(key);
            }
            foreach (var resource in toRemove)
            {
                resourceTypeCount.Remove(resource);
            }
        }
        var keys = new List<ResourceData>(resourceTypeCount.Keys);
        if (keys.Count > 0)
        {
            requestedItem = keys.Random();
            if (!LevelManager.shouldCompeteForResources)
                requestedQuantity = Mathf.FloorToInt(resourceTypeCount[requestedItem] * 0.5f);
            else
            {
                requestedQuantity = Mathf.FloorToInt(Random.Range(1, Mathf.Min(4, resourceTypeCount[requestedItem])));

                // Only propagate in Phase 2
                var cities = FindObjectsByType<City>(FindObjectsSortMode.None);
                foreach (var city in cities)
                {
                    if (city == this) continue;
                    if (city.oxygen <= 0) continue;

                    city.requestedItem = requestedItem;
                    city.requestedQuantity = requestedQuantity;
                    city.timeOfNewRequest = 0;
                }
            }

            timeOfNewRequest = 0;

            if (requestSnd) SoundManager.PlaySound(SoundType.PrimaryFX, requestSnd, 1.0f, 1.0f);
        }
        else
        {
            timeOfNewRequest = 2.0f;
        }
    }

    void UpdateBubbleGfx()
    {
        float t = oxygen / maxOxygen;
        float s = Mathf.Lerp(sizeRange.x, sizeRange.y, t);
        bubbleRenderer.transform.localScale = new Vector3(s, s, s);

        bubbleRenderer.color = bubbleGradient.Evaluate(t);

        if (t < 0.1f)
        {
            cityMaterial.SetColor("_EmissiveColor", new Color(0.0f, 0.0f, 0.0f, 1.0f));
            cityRenderer.sprite = deadCitySprite;
        }
        else
        {
            cityMaterial.SetColor("_EmissiveColor", Color.Lerp(new Color(0.0f, 0.0f, 0.0f, 1.0f), new Color(3.0f, 3.0f, 3.0f, 1.0f), (s - 0.65f) / 0.2f));
            cityRenderer.sprite = liveCitySprite;
        }
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
        if (gatherPivot)
        {
            float r = AdjustRadius(gatherRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(gatherPivot.position, r);
        }
        if (revivePivot)
        {
            float r = AdjustRadius(reviveRadius);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(revivePivot.position, r);
        }
    }

    [Button("Kill")]
    void Kill()
    {
        ChangeOxygen(-maxOxygen * 2.0f);
        UpdateBubbleGfx();
        PopBubble();
    }
}
