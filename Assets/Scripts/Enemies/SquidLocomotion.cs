using Mono.Cecil.Cil;
using NaughtyAttributes;
using System.Linq;
using UnityEngine;

public class SquidLocomotion : Locomotion
{
    [SerializeField] 
    private float       minDistance = 5.0f;
    [SerializeField]    
    private float       linearSpeed = 200.0f;
    [SerializeField]    
    private float       minVelocity = 20.0f;
    [SerializeField]    
    private float       drag = 0.9f;
    [SerializeField]    
    private float       rotationSpeed = 720.0f;
    [SerializeField]    
    private float       bobAmplitude= 10.0f;
    [SerializeField]    
    private float       bobSpeed = 180.0f;

    float       currentSpeed;
    Animator    animator;
    int         animImpulseHash;
    int         animSwimHash;
    float       timeSinceLastImpulse; 
    bool        bobbing;
    Vector3     bobPos;
    float       bobAngle;

    void Start()
    {
        animator = GetComponent<Animator>();
        animImpulseHash = Animator.StringToHash("Impulse");
        animSwimHash = Animator.StringToHash("Swim");

        if (priorityFromInstanceID)
        {
            priority = gameObject.GetInstanceID();
        }
    }

    void FixedUpdate()
    {
        float speedAura = Submarine.GetAura(transform.position, -1);

        timeSinceLastImpulse += Time.fixedDeltaTime;

        // See if we're going anywhere
        bool moving = false;
        float distanceToTarget = 0.0f;

        if (targetPosition.HasValue)
        {
            Vector3 toTarget = targetPosition.Value - ((bobbing) ? (bobPos) : (transform.position));
            distanceToTarget = toTarget.magnitude;

            if (distanceToTarget > minDistance)
            {
                bobbing = false;

                currentSpeed = Mathf.Max(0, currentSpeed - (currentSpeed * drag * Time.fixedDeltaTime));

                var targetRotation = Quaternion.LookRotation(Vector3.forward, toTarget.normalized);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed * speedAura);

                float angle = Quaternion.Angle(transform.rotation, targetRotation);

                if (Mathf.Abs(angle) < 5)
                {
                    var info = animator.GetCurrentAnimatorStateInfo(0);
                    if (currentSpeed < minVelocity)
                    {
                        if ((info.shortNameHash == animSwimHash) && (timeSinceLastImpulse < 1.0f))
                        {
                            float maxDistance = linearSpeed / drag;

                            currentSpeed = linearSpeed * Mathf.Clamp01(distanceToTarget / maxDistance);
                        }
                        else if (info.shortNameHash != animImpulseHash)
                        {
                            animator.SetTrigger("Accelerate");
                            timeSinceLastImpulse = 0;
                        }
                    }
                }

                if ((currentSpeed < (minVelocity + linearSpeed) * 0.5f) && (timeSinceLastImpulse > 1.0f))
                {
                    animator.SetTrigger("Idle");
                }

                if (rb)
                    rb.linearVelocity = currentSpeed * transform.up * speedAura;
                else
                    transform.position = transform.position + currentSpeed * transform.up * Time.fixedDeltaTime * speedAura;
            }
        }

        if (!moving)
        {
            animator.SetTrigger("Idle");

            if (distanceToTarget > 1e-3)
            {
                Vector2 targetPos = Vector3.MoveTowards(transform.position, targetPosition.Value, currentSpeed * Time.fixedDeltaTime * speedAura);

                if (rb)
                    rb.MovePosition(targetPos);
                else
                    transform.position = targetPos;
            }
            else
            {
                currentSpeed = 0.0f;

                if (!bobbing)
                {
                    bobPos = (targetPosition.HasValue) ? (targetPosition.Value) : (transform.position);
                    bobbing = true;
                    bobAngle = 0;
                }

                Vector2 targetPos = bobPos + Vector3.up * bobAmplitude * Mathf.Sin(bobAngle * Mathf.Deg2Rad);
                if (rb)
                    rb.MovePosition(targetPos);
                else
                    transform.position = targetPos;

                bobAngle += Time.fixedDeltaTime * bobSpeed * speedAura;
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, Time.fixedDeltaTime * rotationSpeed * speedAura);
        }

        if (agentAvoidance)
        {
            var enemies = HypertaggedObject.GetInRadius<Locomotion>(avoidanceTag, transform.position.xy(), avoidanceRadius * 2.0f);
            foreach (var enemy in enemies)
            {
                if (enemy == transform) continue;

                Vector3 toEnemy = (enemy.transform.position - transform.position).xy0();
                if (toEnemy.magnitude < avoidanceRadius * 2.0f)
                {
                    float toMove = (avoidanceRadius * 2.0f - toEnemy.magnitude);

                    if (avoidancePriority == enemy.avoidancePriority)
                    {
                        toMove *= 0.5f;
                        transform.position -= toEnemy.normalized * toMove;
                        enemy.transform.position += toEnemy.normalized * toMove;
                    }
                    else if (avoidancePriority < enemy.avoidancePriority)
                    {
                        transform.position -= toEnemy.normalized * toMove;
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (agentAvoidance)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
        }
    }
}
