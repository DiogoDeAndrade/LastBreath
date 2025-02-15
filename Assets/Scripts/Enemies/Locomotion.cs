using NaughtyAttributes;
using System;
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
    public float speedMultiplier = 1.0f;

    protected Vector3?      targetPosition;
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

    public bool hasTarget => targetPosition.HasValue;

    private void OnDrawGizmosSelected()
    {
        if (hasTarget)
        {
            Gizmos.color = new Color(1.0f, 1.0f, 0.0f, 0.5f); ;
            Gizmos.DrawLine(transform.position, targetPosition.Value);
            Gizmos.DrawWireCube(targetPosition.Value, Vector3.one * 5.0f);
            DebugHelpers.DrawTextAt(targetPosition.Value, Vector3.down * 20.0f, 10, Gizmos.color, "Locomotion Target");

        }
        if (agentAvoidance)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
        }
    }

    public virtual void TeleportTo(Vector3 pos)
    {
        this.targetPosition = null;
        transform.position = pos;
        if (rb) rb.MovePosition(pos);
    }
}
