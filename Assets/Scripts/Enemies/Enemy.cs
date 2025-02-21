using Mono.Cecil.Cil;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private enum State { Wander, Pursuit, Search, Fade };

    [SerializeField, Header("Wander")]
    private float           wanderRadius = 200.0f;
    [SerializeField]
    private bool            stayNearSpawn = true;
    [SerializeField]
    private float           wanderSpeedMultiplier = 1.0f;
    [SerializeField, MinMaxSlider(0.0f, 20.0f)]
    private Vector2         delayBetweenWanders;
    [SerializeField]
    private LayerMask       obstacleMask;
    [SerializeField, Header("Pursuit")]
    private float           pursuitRadius = 0.0f;
    [SerializeField, ShowIf(nameof(canPursuit))]
    private float           pursuitMaxRadius = 0.0f;
    [SerializeField, ShowIf(nameof(canPursuit))]
    private float           pursuitSpeedMultiplier = 1.0f;
    [SerializeField, ShowIf(nameof(canPursuit))]
    private Hypertag[]      pursuitTags;
    [SerializeField, Header("Search"), ShowIf(nameof(canPursuit))]
    private float           searchRadius = 0.0f;
    [SerializeField, ShowIf(nameof(canSearch))]
    private float           maxSearchRadius = 0.0f;
    [SerializeField, ShowIf(nameof(canSearch))]
    private float           searchRadiusIncrease = 0.0f;
    [SerializeField, ShowIf(nameof(canSearch))]
    private bool            canChangeTargets = true;
    [SerializeField, ShowIf(nameof(canSearch))]
    private float           searchSpeedMultiplier  =1.0f;
    [SerializeField, ShowIf(nameof(canSearch))]
    private float           maxSearchTime = 20.0f;
    [SerializeField, Header("Death")] 
    private LootList        lootList;
    [SerializeField]
    private Resource        resourcePrefab;
    [SerializeField] 
    private ParticleSystem  deathPS;
    [SerializeField] 
    private GameObject      explosionPrefab;

    State           state = State.Wander;
    SpriteRenderer  spriteRenderer;
    HealthSystem    healthSystem;
    SpriteEffect    spriteEffect;
    Locomotion      locomotion;
    Vector3         spawnPoint;
    float           wanderCooldown;
    HealthSystem    pursuitTarget;
    int             totalFails;
    float           currentSearchRadius;
    float           stateChangeTime;

    bool canPursuit => pursuitRadius > 0;
    bool canSearch => (canPursuit) && (searchRadius > 0);

    float timeSinceStateChange => Time.time - stateChangeTime;

    void Start()
    {
        healthSystem = GetComponent<HealthSystem>();
        spriteEffect = GetComponent<SpriteEffect>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        locomotion = GetComponent<Locomotion>();

        healthSystem.onHit += HealthSystem_onHit;
        healthSystem.onDead += HealthSystem_onDead;

        spawnPoint = transform.position;
    }

    private void HealthSystem_onHit(HealthSystem.DamageType damageType, float damage, Vector3 damagePosition, Vector3 hitDirection, GameObject damageSource)
    {
        spriteEffect.FlashInvert(0.1f);
    }

    private void HealthSystem_onDead(GameObject damageSource)
    {
        if (deathPS) deathPS.Play();
        if (explosionPrefab) Instantiate(explosionPrefab, transform.position, transform.rotation);
        spriteRenderer.FadeTo(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.1f);

        if (lootList)
        {
            var item = lootList.Get();
            if (item != null)
            {
                var res = Instantiate(resourcePrefab, transform.position, transform.rotation);
                res.SetType(item);
                res.RandomThrow();
                res.duration = 20.0f;
            }
        }

        var attacks = GetComponentsInChildren<ProximityAttack>();
        foreach (var attack in attacks)
        {
            attack.enabled = false;
        }

        Destroy(gameObject, 1.0f);
    }

    private void Update()
    {
        switch (state)
        {
            case State.Wander:
                WanderState();
                break;
            case State.Pursuit:
                PursuitState();
                break;
            case State.Search:
                SearchState();
                break;
            default:
                break;
        }
    }

    void WanderState()
    {
        if (!locomotion.hasTarget)
        {
            wanderCooldown -= Time.deltaTime;
            if (wanderCooldown <= 0.0f)
            {
                SelectWanderPoint();
            }
        }
        if (pursuitRadius > 0)
        {
            FindPursuitTarget();
        }
    }

    void FindPursuitTarget()
    {
        var candidates = HypertaggedObject.GetInRadius<HealthSystem>(pursuitTags, transform.position.xy(), pursuitRadius);

        float minDist = float.MaxValue;
        foreach (var candidate in candidates)
        {
            if (!healthSystem.faction.IsHostile(candidate.faction)) continue;

            float dist = Vector3.Distance(candidate.transform.position, transform.position);
            if (dist < minDist)
            {
                if (CheckLOS(transform.position, candidate.transform.position))
                {
                    ChangeState(State.Pursuit);
                    pursuitTarget = candidate;
                }
            }
        }
    }

    void SelectWanderPoint()
    {
        int nTries = 0;
        while (nTries < 20)
        {
            Vector2 candidate;
            if (stayNearSpawn)
            {
                candidate = spawnPoint.xy() + UnityEngine.Random.insideUnitCircle * wanderRadius;
            }
            else
            {
                candidate = transform.position.xy() + UnityEngine.Random.insideUnitCircle * wanderRadius;
            }

            // Check LOS
            if (CheckLOS(transform.position.xy(), candidate))
            {
                wanderCooldown = delayBetweenWanders.Random();
                locomotion.speedMultiplier = wanderSpeedMultiplier;
                locomotion.MoveTo(candidate);
                totalFails = 0;
                return;
            }
            nTries++;
        }

        totalFails += 20;

        if (totalFails > 200)
        {
            // Fade out and move to spawn point
            StartCoroutine(TeleportToSpawnPointCR());
        }
    }

    IEnumerator TeleportToSpawnPointCR()
    {
        ChangeState(State.Fade);

        float originalAlpha = spriteRenderer.color.a;

        var tween = spriteRenderer.FadeTo(spriteRenderer.color.ChangeAlpha(0.0f), 0.25f);
        yield return new WaitForTween(tween);

        locomotion.TeleportTo(spawnPoint);

        tween = spriteRenderer.FadeTo(spriteRenderer.color.ChangeAlpha(originalAlpha), 0.25f);
        yield return new WaitForTween(tween);

        ChangeState(State.Wander);
    }

    void PursuitState()
    {
        if (pursuitTarget == null)
        {
            // Back to wander
            ChangeState(State.Wander);
            return;
        }

        Vector2 targetPos = pursuitTarget.transform.position;

        if (CheckLOS(transform.position.xy(), targetPos, pursuitMaxRadius))
        {            
            locomotion.speedMultiplier = pursuitSpeedMultiplier;
            locomotion.MoveTo(targetPos);
        }
        else
        {
            if (!locomotion.hasTarget)
            {
                // Arrive on last seen position, basically
                // Change to search state, or wander state
                if (searchRadius > 0)
                {
                    ChangeState(State.Search);
                    currentSearchRadius = searchRadius;
                }
                else ChangeState(State.Wander);
            }
        }
    }

    void SearchState()
    {
        if (!locomotion.hasTarget)
        {
            SelectSearchPoint();
        }

        if (timeSinceStateChange > maxSearchTime)
        {
            ChangeState(State.Wander);
        }

        // Check if target is available, etc
        if (pursuitTarget != null)
        {
            float dist = Vector3.Distance(pursuitTarget.transform.position, transform.position);
            if (dist < ((pursuitMaxRadius > 0) ? (pursuitMaxRadius) : (float.MaxValue)))
            {
                if (CheckLOS(transform.position, pursuitTarget.transform.position))
                {
                    ChangeState(State.Pursuit);
                }
            }
        }

        if (canChangeTargets)
        {
            FindPursuitTarget();
        }
    }

    void SelectSearchPoint()
    {
        int nTries = 0;
        while (nTries < 20)
        {
            Vector2 candidate;
            candidate = transform.position.xy() + UnityEngine.Random.insideUnitCircle * currentSearchRadius;

            // Check LOS
            if (CheckLOS(transform.position.xy(), candidate))
            {
                locomotion.speedMultiplier = searchSpeedMultiplier;
                locomotion.MoveTo(candidate);
                totalFails = 0;
                currentSearchRadius += searchRadiusIncrease;
                if (maxSearchRadius > 0)
                {
                    currentSearchRadius = Mathf.Min(currentSearchRadius, maxSearchRadius);
                }
                return;
            }
            nTries++;
        }

        totalFails += 20;

        if (totalFails > 500)
        {
            // Fade out and move to spawn point
            StartCoroutine(TeleportToSpawnPointCR());
        }
    }

    bool CheckLOS(Vector2 p1, Vector2 p2, float maxDist = 0.0f)
    {
        var dir = p2 - p1;
        var dist = dir.magnitude;
        if ((dist > 0) && ((dist < maxDist) || (maxDist == 0.0f)))
        {
            var collider = Physics2D.Raycast(p1, dir.normalized, dist, obstacleMask);
            if (collider.collider == null)
            {
                return true;
            }
        }

        return false;
    }

    void ChangeState(State state)
    {
        this.state = state;
        stateChangeTime = Time.time;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
        DebugHelpers.DrawTextAt(transform.position + Vector3.right * wanderRadius, Vector3.up * 20.0f, 10, Gizmos.color, "Wander Radius");
        if (pursuitRadius > 0)
        {
            Gizmos.color = new Color(1, 0.5f, 0.1f, 1.0f);
            Gizmos.DrawWireSphere(transform.position, pursuitRadius);
            DebugHelpers.DrawTextAt(transform.position + Vector3.right * pursuitRadius, Vector3.down * 20.0f, 10, Gizmos.color, "Pursuit Radius");

            if (pursuitMaxRadius > 0)
            {
                Gizmos.color = new Color(1, 0.5f, 0.1f, 0.5f);
                Gizmos.DrawWireSphere(transform.position, pursuitMaxRadius);
                DebugHelpers.DrawTextAt(transform.position + Vector3.right * pursuitMaxRadius, Vector3.down * 20.0f, 10, Gizmos.color, "Pursuit Max Radius");
            }
        }
        if (searchRadius > 0)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, searchRadius);
            DebugHelpers.DrawTextAt(transform.position - Vector3.right * searchRadius, Vector3.up * 20.0f, 10, Gizmos.color, "Search Radius");
        }

        switch (state)
        {
            case State.Wander:
                DebugHelpers.DrawTextAt(transform.position + Vector3.right * 16.0f, Vector3.up * 20.0f, 10, Color.green, "State Wander");
                break;
            case State.Pursuit:
                DebugHelpers.DrawTextAt(transform.position + Vector3.right * 16.0f, Vector3.up * 20.0f, 10, Color.red, "State Pursuit");
                break;
            case State.Search:
                DebugHelpers.DrawTextAt(transform.position + Vector3.right * 16.0f, Vector3.up * 20.0f, 10, Color.yellow, "State Search");
                break;
            case State.Fade:
                DebugHelpers.DrawTextAt(transform.position + Vector3.right * 16.0f, Vector3.up * 20.0f, 10, Color.white, "State Fade");
                break;
            default:
                break;
        }
    }
#endif
}

#if UNITY_6000_0_OR_NEWER

[Serializable]
public class EnemyProbList : ProbList<Enemy> { }

#endif
