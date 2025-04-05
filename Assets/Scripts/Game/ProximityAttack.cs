using System.Linq;
using UnityEngine;
using UC;

public abstract class ProximityAttack : MonoBehaviour
{
    [SerializeField, Header("Proximity Attack")]
    private float       attackRange;
    [SerializeField]
    private float       timeToAttack;
    [SerializeField]
    private float       attackCooldown;
    [SerializeField]
    private Hypertag[]  targetTags;

    HealthSystem        currentTarget;

    float           attackTimer;
    float           cooldownTimer;
    HealthSystem    healthSystem;

    void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (cooldownTimer > 0.0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer > 0.0f) return;
        }
        var             candidateTargets = HypertaggedObject.GetInRadius<HealthSystem>(targetTags, transform.position.xy(), attackRange).ToList();
        if (currentTarget != null)
        {
            if (candidateTargets.IndexOf(currentTarget) != -1)
            {
                // Still a target, decrement attack time
                attackTimer += Time.deltaTime;
                if (attackTimer > timeToAttack)
                {
                    TriggerAttack(currentTarget);                    
                }
            }
            else
            {
                // Lost the target
                attackTimer = 0.0f;
                currentTarget = null;
            }
        }
        else
        {
            // Find closest target from candidates
            if (candidateTargets.Count == 0) return;

            candidateTargets.Sort((h1, h2) => Vector3.Distance(transform.position, h1.transform.position).CompareTo(Vector3.Distance(transform.position, h2.transform.position)));

            foreach (var candidate in candidateTargets)
            {
                if (healthSystem.faction.IsHostile(candidate.faction))
                {
                    currentTarget = candidateTargets.PopFirst();
                    attackTimer = 0.0f;
                    break;
                }
            }
        }
    }

    void TriggerAttack(HealthSystem hs)
    {
        if (Execute(currentTarget))
        {
            attackTimer = 0.0f;
            currentTarget = null;
            cooldownTimer = attackCooldown;
        }
    }

    protected abstract bool Execute(HealthSystem target);

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        DebugHelpers.DrawTextAt(transform.position + Vector3.left * attackRange, Vector3.up * 20.0f, 10, Gizmos.color, "Proximity Attack\nRange");
    }
}
