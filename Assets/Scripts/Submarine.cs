using NaughtyAttributes;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField, Header("Input")] 
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))] 
    private InputControl    moveControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private InputControl    shootControl;

    private Vector2         movementVector;
    private Rigidbody2D     rb;
    private HealthSystem    healthSystem;
    private SpriteEffect    spriteEffect;
    private int             _ammo;
    private float           cooldownTimer;

    public float normalizedAmmo => _ammo / (float)maxTorpedo;
    public float ammo => _ammo;

    public int playerId { get { return _playerId; } set { _playerId = value; } }

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

        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.onHit += HealthSystem_onHit;
        healthSystem.onDead += HealthSystem_onDead;

        _ammo = maxTorpedo;
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

        Destroy(gameObject);
    }

    private void HealthSystem_onHit(float damage, Vector3 damagePosition, Vector3 damageNormal)
    {
        rb.linearVelocity = damageNormal * maxSpeed * 0.5f;
        spriteEffect.FlashColor(0.1f, Color.red);
    }

    void FixedUpdate()
    {
        Vector2 velocity = rb.linearVelocity;

        velocity = velocity - velocity * drag * Time.fixedDeltaTime;

        if (!healthSystem.isInvulnerable)
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

        healthSystem.DealDamage(damage, collision.contacts[0].point, collision.contacts[0].normal);

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
}
