using NaughtyAttributes;
using UnityEngine;

public class Locomotion : MonoBehaviour
{
    [SerializeField]
    protected bool  agentAvoidance = false;
    [SerializeField, ShowIf(nameof(agentAvoidance))]
    protected float avoidanceRadius;
    [SerializeField, ShowIf(nameof(agentAvoidance))]
    protected Hypertag avoidanceTag;
    [SerializeField, ShowIf(nameof(agentAvoidance))]
    protected bool priorityFromInstanceID;
    [SerializeField, ShowIf(nameof(needPriority))]
    protected int priority;
    protected bool needPriority => agentAvoidance && !priorityFromInstanceID;
    public int avoidancePriority => priority;

    protected Vector3       targetPosition;
    protected Rigidbody2D   rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveTo(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    public void MoveTo(Vector3 targetPosition, bool checkLOS, LayerMask layerMask)
    {
        if (checkLOS)
        {
            if ((agentAvoidance) && (avoidanceRadius > 0))
            {
                var toTarget = (targetPosition - transform.position);
                var hit = Physics2D.CircleCast(transform.position, avoidanceRadius, toTarget.normalized, toTarget.magnitude, layerMask);
                if (hit.collider != null) return;
            }
            else
            {
                var toTarget = (targetPosition - transform.position);
                var hit = Physics2D.Raycast(transform.position, toTarget.normalized, toTarget.magnitude, layerMask);
                if (hit.collider != null) return;
            }
        }

        this.targetPosition = targetPosition;
    }
}
