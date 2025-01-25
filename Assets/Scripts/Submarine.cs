using NaughtyAttributes;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using static HealthSystem;

public class Submarine : MonoBehaviour
{
    [SerializeField] 
    private int             _playerId = 0;
    [SerializeField]
    private float           maxSpeed = 200.0f; 
    [SerializeField]
    private float           acceleration = 200.0f; 
    [SerializeField]
    private float           drag = 1.0f;
    [SerializeField, Header("Damage")]
    private float           collisionDamagePerSpeed = 0.1f;
    [SerializeField, Tooltip("Cosine of the angle tolerance for the cockpit (for example, 0.707 would make any damage in a 45 degree angle in front of the ship being critical)")]
    private float           cockpitDamageThreshould = 0.9f;
    [SerializeField]
    private float           criticalMultiplier = 2.0f;
    [SerializeField]
    private GameObject      explosionObject;
    [SerializeField]
    private GameObject[]    debries;
    [SerializeField, Header("Torpedo")]
    private int             maxTorpedo = 3;
    [SerializeField]
    private float           shootCooldown = 0.5f;
    [SerializeField]
    private Transform       shootPoint;
    [SerializeField]
    private Torpedo         torpedoPrefab;
    [SerializeField, Header("Gathering")]
    private float           gatherRadius;
    [SerializeField]
    private Transform       gatherPivot;
    [SerializeField]
    private LayerMask       gatherMask;
    [SerializeField]
    private LineRenderer    rope;
    [SerializeField]
    private Resource        resourcePrefab;
    [SerializeField, Header("Input")] 
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))] 
    private InputControl    moveControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private InputControl    shootControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private InputControl    gatherControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private InputControl    dropControl;

    private Vector2         movementVector;
    private Rigidbody2D     rb;
    private HealthSystem    healthSystem;
    private SpriteEffect    spriteEffect;
    private int             _ammo;
    private float           cooldownTimer;
    private float           noControlTime;
    private Resource        grabResource;
    private float           grabT;
    private bool            grabGoingOut;
    private Vector3         grabOriginalPosition;
    private ResourceData    inventoryType;
    private int             inventoryQuantity;

    private Tweener.BaseInterpolator healthGainEffect;
    private Tweener.BaseInterpolator hitFlash;

    public float normalizedAmmo => _ammo / (float)maxTorpedo;
    public float ammo => _ammo;
    public float maxAmmo => maxTorpedo;

    public int          playerId { get { return _playerId; } set { _playerId = value; } }
    public ResourceData item => inventoryType;
    public int          itemCount => inventoryQuantity;


    void Start()
    {
        if (_playerId >= GameManager.Instance.numPlayers)
        {
            Destroy(gameObject);
            return;
        }

        var pd = GameManager.Instance.GetPlayerData(_playerId);

        spriteEffect = GetComponent<SpriteEffect>();
        SubCustomization playerCustomization = GetComponent<SubCustomization>();
        if (playerCustomization)
        {
            playerCustomization.SetColors(pd.hullColor, pd.stripeColor, pd.cockpitColor);
            foreach (var d in debries)
            {
                var effect = d.GetComponent<SpriteEffect>();
                effect?.SetRemap(spriteEffect.GetRemap());
            }
        }

        MasterInputManager.SetupInput(_playerId, playerInput);
        moveControl.playerInput = playerInput;
        shootControl.playerInput = playerInput;
        gatherControl.playerInput = playerInput;
        dropControl.playerInput = playerInput;

        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.onHit += HealthSystem_onHit;
        healthSystem.onHeal += HealthSystem_onHeal;
        healthSystem.onDead += HealthSystem_onDead;

        _ammo = maxTorpedo;
    }

    private void HealthSystem_onHeal(float healthGain)
    {
        if ((healthGainEffect == null) || (healthGainEffect.isFinished))
        {
            healthGainEffect = spriteEffect.FlashColor(0.5f, Color.green);
        }
    }

    private void HealthSystem_onDead()
    {
        if (explosionObject) Instantiate(explosionObject, transform.position, transform.rotation);
        foreach (var d in debries)
        {
            d.gameObject.SetActive(true);
        }
        var collider = GetComponent<Collider2D>();
        collider.enabled = false;
        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;

        DropAll(false);
        if (grabResource)
        {
            grabResource.RandomThrow();
            grabResource = null;
        }

        Destroy(gameObject);
    }

    private void HealthSystem_onHit(DamageType damageType, float damage, Vector3 damagePosition, Vector3 damageNormal)
    {
        if (damageNormal.magnitude > 0)
        {
            rb.linearVelocity = damageNormal * maxSpeed * 0.5f;
        }
        if ((hitFlash == null) || (hitFlash.isFinished))
        {
            hitFlash = spriteEffect.FlashColor(0.1f, Color.red);
        }
        if (damageType != DamageType.OverTime)
        {
            noControlTime = healthSystem.invulnerabilityTime * 0.4f;
        }
    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;

        velocity = velocity - velocity * drag * Time.fixedDeltaTime;

        if (noControlTime > 0) noControlTime -= Time.fixedDeltaTime;
        else
        {
            velocity = velocity + movementVector * acceleration * Time.fixedDeltaTime;
        }
        
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0, maxSpeed);

        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        movementVector = moveControl.GetAxis2();

        if (!healthSystem.isInvulnerable)
        {
            var velocity = movementVector;
            if (movementVector.sqrMagnitude > 1e-3)
            {
                var direction = movementVector.normalized;

                var targetRotation = Quaternion.LookRotation(Vector3.forward, new Vector2(-velocity.y, velocity.x));

                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * 720.0f);
            }
        }

        if (shootControl.IsDown())
        {
            Shoot();
        }
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (gatherControl.IsDown())
        {
            Gather();
        }

        if (dropControl.IsDown())
        {
            DropAll(false);
        }

        if (grabResource)
        {
            float grabSpeed = 5.0f;

            rope.enabled = true;

            rope.useWorldSpace = true;
            if (grabGoingOut)
            {
                grabT = Mathf.Clamp01(grabT + Time.deltaTime * grabSpeed);
                rope.SetPositions(new Vector3[2] { gatherPivot.position, Vector3.Lerp(gatherPivot.position, grabResource.transform.position, grabT) });

                if (grabT >= 1.0f)
                {
                    grabT = 0.0f;
                    grabGoingOut = false;
                    grabOriginalPosition = grabResource.transform.position;
                }
            }
            else
            {
                grabT = Mathf.Clamp01(grabT + Time.deltaTime * grabSpeed);
                grabResource.transform.position = Vector3.Lerp(grabOriginalPosition, gatherPivot.position, Ease.Sqr(grabT));
                rope.SetPositions(new Vector3[2] { gatherPivot.position, grabResource.transform.position });

                if (grabT >= 1.0f)
                {
                    // Done, gathered
                    inventoryType = grabResource.data;
                    inventoryQuantity++;

                    Destroy(grabResource.gameObject);
                    grabResource = null;
                }
            }
        }
        else
        {
            rope.enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var damage = collision.relativeVelocity.magnitude * collisionDamagePerSpeed;

        // Check if there was a critical hit
        Vector2 forwardVector = transform.right.xy();
        Vector2 toDamage = (collision.contacts[0].point - transform.position.xy()).normalized;
        if (Vector3.Dot(forwardVector, toDamage) > cockpitDamageThreshould)
        {
            damage *= criticalMultiplier;
        }

        healthSystem.DealDamage(DamageType.Burst, damage, collision.contacts[0].point, collision.contacts[0].normal);

        // Check if collision was with another thing with health
    }

    void Shoot()
    {
        if (_ammo <= 0) return;
        if (cooldownTimer > 0) return;

        _ammo--;
        cooldownTimer = shootCooldown;

        var torpedo = Instantiate(torpedoPrefab, shootPoint.position, shootPoint.rotation);
        torpedo.SetPlayerId(playerId);
    }

    public void AddAmmo(int delta)
    {
        _ammo += delta;
        if (_ammo > maxTorpedo) _ammo = maxTorpedo;
    }

    void Gather()
    {
        var colliders = Physics2D.OverlapCircleAll(gatherPivot.position, gatherRadius, gatherMask);
        foreach (var collider in colliders)
        {
            var resource = collider.GetComponent<Resource>();
            if (resource)
            {
                if (CanGrab(resource.data))
                {
                    grabResource = resource;
                    grabResource.Grab();
                    grabGoingOut = true;
                    grabT = 0.0f;
                    return;
                }
            }
        }
    }

    bool CanGrab(ResourceData data)
    {
        // Can't grab if still grabbing
        if (grabResource != null) return false;
        // Can't grab if already have an item of a different type
        if ((inventoryType != null) && (inventoryType != data)) return false;

        return true;
    }

    private void OnDrawGizmos()
    {
        if (gatherPivot)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(gatherPivot.position, gatherRadius);
        }
    }
    public void DropAll(bool atBase)
    {
        if (inventoryType == null) return;

        if (!atBase)
        {
            for (int i = 0; i < inventoryQuantity; i++)
            {
                var res = Instantiate(resourcePrefab, transform.position, transform.rotation);
                res.SetType(inventoryType);
                res.RandomThrow();
            }
        }

        inventoryType = null;
        inventoryQuantity = 0;
    }
}
