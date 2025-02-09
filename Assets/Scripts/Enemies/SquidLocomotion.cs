using UnityEngine;

public class SquidLocomotion : Locomotion
{
    [SerializeField] private float minDistance = 5.0f;
    [SerializeField] private float linearSpeed = 200.0f;
    [SerializeField] private float minVelocity = 20.0f;
    [SerializeField] private float drag = 0.9f;
    [SerializeField] private float rotationSpeed = 720.0f;
    [SerializeField] private float bobAmplitude= 10.0f;
    [SerializeField] private float bobSpeed = 180.0f;

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
    }

    void FixedUpdate()
    {
        timeSinceLastImpulse += Time.fixedDeltaTime;

        // See if we're going anywhere
        Vector3 toTarget = targetPosition - transform.position;
        float   distanceToTarget = toTarget.magnitude;

        if (distanceToTarget > minDistance)
        {
            bobbing = false;

            currentSpeed = Mathf.Max(0, currentSpeed - (currentSpeed * drag * Time.fixedDeltaTime));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Vector3.forward, toTarget.normalized), Time.fixedDeltaTime * rotationSpeed);

            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (currentSpeed < minVelocity)
            {
                if ((info.shortNameHash == animSwimHash) && (timeSinceLastImpulse < 1.0f))
                {
                    currentSpeed = linearSpeed;
                }
                else if (info.shortNameHash != animImpulseHash)
                {
                    animator.SetTrigger("Accelerate");
                    timeSinceLastImpulse = 0;
                }
            }

            transform.position = transform.position + currentSpeed * transform.up * Time.fixedDeltaTime;
        }
        else
        {
            if (distanceToTarget > 1e-3)
            {
                //transform.position = Vector3.Lerp(transform.position, targetPosition, Time.fixedDeltaTime);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentSpeed * Time.fixedDeltaTime);
            }
            else
            {
                currentSpeed = 0.0f;

                if (!bobbing)
                {
                    bobPos = targetPosition;
                    bobbing = true;
                    bobAngle = 0;
                    animator.SetTrigger("Idle");
                }

                transform.position = bobPos + Vector3.up * bobAmplitude * Mathf.Sin(bobAngle * Mathf.Deg2Rad);
                bobAngle += Time.fixedDeltaTime * bobSpeed;
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.identity, Time.fixedDeltaTime * rotationSpeed);
        }
    }
}
