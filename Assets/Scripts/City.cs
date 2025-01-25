using NaughtyAttributes;
using System.Collections.Generic;
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
    [SerializeField]
    private Transform   gatherPivot;
    [SerializeField]
    private float       gatherRadius = 50.0f;
    [SerializeField]
    private float       oxygenPerResource = 50.0f;

    [SerializeField, Header("Visuals")]
    private SpriteRenderer  bubbleRenderer;
    [SerializeField]
    private Gradient    bubbleGradient;
    [SerializeField]
    private RequestUI   requestUI;

    float           penaltyTimer = 0.0f;
    Submarine       player;
    float           oxygen;
    float           ammoReload;
    ResourceData    requestedItem;
    int             requestedQuantity;
    float           timeOfNewRequest;

    void Start()
    {
        oxygen = startOxygen;
        timeOfNewRequest = 5.0f;
    }

    void Update()
    {
        if (oxygen > 0)
        {
            ChangeOxygen(-oxygenLossPerSecond * Time.deltaTime);
            if (oxygen <= 0.0f)
            {
                PopBubble();
                requestUI.UpdateUI(null, 0);
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

                                CancelAllRequests();
                            }
                        }
                        else
                        {
                            oxygenCount = oxygenPerResource * player.itemCount * player.item.valueMultiplier;
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

    void CancelAllRequests()
    {
        if (GameManager.GetPhase() == GameManager.Phase.Phase1)
        {
            CancelRequest();
        }
        else
        {
            var cities = FindObjectsByType<City>(FindObjectsSortMode.None);
            foreach (var city in cities)
            {
                city.CancelRequest();
            }
        }
    }

    void CancelRequest()
    {
        requestedItem = null;
        requestedQuantity = 0;
        timeOfNewRequest = 4.0f;

        UpdateRequestUI();
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

        if (GameManager.GetPhase() == GameManager.Phase.Phase1)
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
            if (GameManager.GetPhase() == GameManager.Phase.Phase1)
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

                    city.UpdateRequestUI();
                }
            }

            timeOfNewRequest = 0;

            UpdateRequestUI();
        }
        else
        {
            timeOfNewRequest = 2.0f;
        }
    }

    void UpdateRequestUI()
    {
        requestUI.UpdateUI(requestedItem, requestedQuantity);
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
