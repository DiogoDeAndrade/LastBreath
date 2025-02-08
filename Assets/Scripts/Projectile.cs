using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Hardware;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class Projectile : MonoBehaviour
{
    [SerializeField] 
    private float           maxSpeed = 250.0f;
    [SerializeField] 
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

    private int         _playerId;
    private Rigidbody2D rb;
    private float       timer;
    private float       _speedModifier = 1.0f;
    private float       _damageModifier = 1.0f;
    private bool        targetAcquired;

    bool isTracker => trackingTags.Count > 0;

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

        float ms = maxSpeed;
        ms *= Submarine.GetAura(transform.position, _playerId);

        velocity = transform.right * Mathf.Clamp(speed + acceleration * Time.fixedDeltaTime, 0.0f, ms) * _speedModifier;
        rb.linearVelocity = velocity;
    }

    private void Update()
    {
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
        var toSub = (target.position - trackPoint.position).normalized;
        float angle = Vector2.Angle(trackPoint.right, toSub);
        if (angle < angularTolerance)
        {
            // Follow this
            var direction = Quaternion.LookRotation(Vector3.forward, toSub.PerpendicularXY());
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
        // Check collision with player, do nothing to self player
        var submarine = collision.GetComponent<Submarine>();
        if (submarine != null)
        {
            if (submarine.playerId == _playerId) return;            
        }
        var torpedo = collision.GetComponent<Projectile>();
        if (torpedo != null)
        {
            if (torpedo._playerId == _playerId) return;
        }

        var health = collision.GetComponent<HealthSystem>();
        health?.DealDamage(HealthSystem.DamageType.Burst, damage * _damageModifier, transform.position, Vector3.zero, gameObject);

        if (explosionPrefab) Instantiate(explosionPrefab, transform.position, transform.rotation);

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if ((isTracker) && (trackPoint != null))
        {
            Handles.color = new Color(1.0f, 1.0f, 0.0f, 0.1f);
            Handles.DrawSolidArc(trackPoint.position, Vector3.forward, transform.right.RotateZ(-angularTolerance), angularTolerance * 2.0f, rangeTolerance);
        }
    }
#endif
}
