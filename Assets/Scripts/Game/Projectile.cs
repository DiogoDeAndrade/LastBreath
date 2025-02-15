using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Projectile : MonoBehaviour
{
    [SerializeField] 
    private float           maxSpeed = 250.0f;
    [SerializeField]
    private bool            useAcceleration;
    [SerializeField, ShowIf(nameof(useAcceleration))] 
    private float           acceleration = 250.0f;
    [SerializeField] 
    private float           duration = 10.0f;
    [SerializeField] 
    private float           damage = 50.0f;
    [SerializeField] 
    private List<Hypertag>  trackingTags;
    [SerializeField, ShowIf(nameof(isTracker))]
    private Transform       trackPoint;
    [SerializeField, ShowIf(nameof(isTracker))]
    private float           angularTolerance = 45.0f;
    [SerializeField, ShowIf(nameof(isTracker))]
    private float           rangeTolerance = 100.0f;
    [SerializeField, ShowIf(nameof(isTracker))]
    private float           rotationSpeed = 360.0f;
    [SerializeField, ShowIf(nameof(isTracker))]
    private AudioClip       sonarSnd;
    [SerializeField] 
    private GameObject      explosionPrefab;

    private int             _playerId;
    private Rigidbody2D     rb;
    private TrailRenderer   trailRenderer;
    private HealthSystem    healthSystem;
    private float           timer;
    private float           _speedModifier = 1.0f;
    private float           _damageModifier = 1.0f;
    private bool            targetAcquired;

    bool isTracker => (trackingTags != null) ? (trackingTags.Count > 0) : (false);
    bool isDead => ((trailRenderer) && (!trailRenderer.emitting));

    public float speedModifier
    {
        get { return _speedModifier; }
        set { _speedModifier = value; }
    }

    public float damageModifier
    {
        get { return _damageModifier; }
        set { _damageModifier = value; }
    }


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (_speedModifier != 1.0f)
        {
            var effect = GetComponent<SpriteEffect>();
            if (effect) effect.SetOutline(1.0f, Color.red);
        }
        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem)
            healthSystem.onDead += HealthSystem_onDead;
        trailRenderer = GetComponent<TrailRenderer>();

        timer = 0.0f;
    }

    private void HealthSystem_onDead(GameObject damageSource)
    {
        SelfDestruct();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        timer += Time.deltaTime;
        if (timer > duration)
        {
            SelfDestruct();

            return;
        }
        if (useAcceleration)
        {
            var velocity = rb.linearVelocity;
            var speed = velocity.magnitude;

            float ms = maxSpeed;
            ms *= Submarine.GetAura(transform.position, _playerId);

            velocity = transform.right * Mathf.Clamp(speed + acceleration * Time.fixedDeltaTime, 0.0f, ms) * _speedModifier;
            rb.linearVelocity = velocity;
        }
        else
        {
            float ms = maxSpeed;
            ms *= Submarine.GetAura(transform.position, _playerId);
            rb.linearVelocity = transform.right * ms * _speedModifier;
        }
    }

    private void Update()
    {
        if (isDead) return;

        if ((isTracker) && (trackPoint != null))
        {
            var overlaps = Physics2D.OverlapCircleAll(trackPoint.position, rangeTolerance);
            foreach (var overlap in overlaps)
            {
                var htag = overlap.GetComponent<HypertaggedObject>();
                if (htag == null) continue;
                if (!htag.HasAnyHypertag(trackingTags)) continue;

                Transform validTarget = overlap.transform;

                var sub = overlap.GetComponent<Submarine>();
                if ((sub) && (sub.playerId == _playerId)) continue;
                var projectile = overlap.GetComponent<Projectile>();
                if ((projectile) && (projectile._playerId == _playerId)) continue;

                if (Track(validTarget))
                {
                    if (!targetAcquired)
                    {
                        SoundManager.PlaySound(SoundType.PrimaryFX, sonarSnd);
                        targetAcquired = true;
                    }
                    return;
                }
            }
            targetAcquired = false;
        }
    }

    bool Track(Transform target)
    {
        var toTarget = (target.position - trackPoint.position).normalized;
        float angle = Vector2.Angle(trackPoint.right, toTarget);
        if (angle < angularTolerance)
        {
            // Follow this
            var direction = Quaternion.LookRotation(Vector3.forward, toTarget.PerpendicularXY());
            transform.rotation = Quaternion.RotateTowards(transform.rotation, direction, _speedModifier * rotationSpeed * Time.deltaTime);

            return true;
        }

        return false;
    }

    public void SetPlayerId(int playerId)
    {
        _playerId = playerId;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        // Check collision with player, do nothing to self player
        float selfDamage = 1000.0f;

        var submarine = collision.GetComponent<Submarine>();
        if (submarine != null)
        {
            if (submarine.playerId == _playerId) return;            
        }
        var projectile = collision.GetComponent<Projectile>();
        if (projectile != null)
        {
            if (projectile._playerId == _playerId) return;
            // Nothing should happen with the projectile hit, the projectil will deal damage to this one instead
            selfDamage = 0.0f;
        }

        var otherHealthSystem = collision.GetComponent<HealthSystem>();
        otherHealthSystem?.DealDamage(HealthSystem.DamageType.Burst, damage * _damageModifier, transform.position, Vector3.zero, gameObject);

        if (healthSystem)
        {
            healthSystem.DealDamage(HealthSystem.DamageType.Burst, selfDamage, transform.position, Vector3.zero, collision.gameObject);
        }
        else
        {
            SelfDestruct();
        }
    }

    void SelfDestruct()
    {
        float destroyTime = 0.0f;

        if (explosionPrefab) Instantiate(explosionPrefab, transform.position, transform.rotation);
        if (trailRenderer)
        {
            trailRenderer.emitting = false;
            destroyTime = 0.2f;
        }

        if (destroyTime > 0) Destroy(gameObject, destroyTime);
        else Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if ((isTracker) && (trackPoint != null))
        {
            Handles.color = new Color(1.0f, 1.0f, 0.0f, 0.1f);
            Handles.DrawSolidArc(trackPoint.position, Vector3.forward, transform.right.RotateZ(-angularTolerance), angularTolerance * 2.0f, rangeTolerance);
        }
    }
#endif
}
