using Mono.Cecil;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using static HealthSystem;

public class Torpedo : MonoBehaviour
{
    [SerializeField] 
    private float       maxSpeed = 250.0f;
    [SerializeField] 
    private float       acceleration = 250.0f;
    [SerializeField] 
    private float       duration = 10.0f;
    [SerializeField] 
    private float       damage = 50.0f;
    [SerializeField] 
    private bool        tracker;
    [SerializeField, ShowIf(nameof(tracker))]
    private Transform   trackPoint;
    [SerializeField, ShowIf(nameof(tracker))]
    private float       angularTolerance = 45.0f;
    [SerializeField, ShowIf(nameof(tracker))]
    private float       rangeTolerance = 100.0f;
    [SerializeField, ShowIf(nameof(tracker))]
    private float       rotationSpeed = 360.0f;
    [SerializeField] 
    private GameObject  explosionPrefab;

    private int         _playerId;
    private Rigidbody2D rb;
    private float       timer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        timer = 0.0f;
    }

    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (timer > duration)
        {
            if (explosionPrefab) Instantiate(explosionPrefab, transform.position, transform.rotation);

            Destroy(gameObject);
            return;
        }
        var velocity = rb.linearVelocity;
        var speed = velocity.magnitude;

        velocity = transform.right * Mathf.Clamp(speed + acceleration * Time.fixedDeltaTime, 0.0f, maxSpeed);
        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        if ((tracker) && (trackPoint != null))
        {
            var overlaps = Physics2D.OverlapCircleAll(trackPoint.position, rangeTolerance);
            foreach (var overlap in overlaps)
            {
                var sub = overlap.GetComponent<Submarine>();
                if (sub != null)
                {
                    if (sub.playerId != _playerId)
                    {
                        // Check angle
                        var toSub = (sub.transform.position - trackPoint.position).normalized;
                        float angle = Vector2.Angle(trackPoint.right, toSub);
                        if (angle < angularTolerance)
                        {
                            // Follow this
                            var direction = Quaternion.LookRotation(Vector3.forward, toSub.PerpendicularXY());
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, direction, rotationSpeed * Time.deltaTime);

                            return;
                        }
                    }
                }
            }
        }
    }

    public void SetPlayerId(int playerId)
    {
        _playerId = playerId;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check collision with player, do nothing to self player
        var submarine = collision.GetComponent<Submarine>();
        if (submarine != null)
        {
            if (submarine.playerId == _playerId) return;            
        }
        var torpedo = collision.GetComponent<Torpedo>();
        if (torpedo != null)
        {
            if (torpedo._playerId == _playerId) return;
        }

        var health = collision.GetComponent<HealthSystem>();
        health?.DealDamage(DamageType.Burst, damage, transform.position, Vector3.zero);

        if (explosionPrefab) Instantiate(explosionPrefab, transform.position, transform.rotation);

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if ((tracker) && (trackPoint != null))
        {
            Handles.color = new Color(1.0f, 1.0f, 0.0f, 0.1f);
            Handles.DrawSolidArc(trackPoint.position, Vector3.forward, transform.right.RotateZ(-angularTolerance), angularTolerance * 2.0f, rangeTolerance);
        }
    }
}
