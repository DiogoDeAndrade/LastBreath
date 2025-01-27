using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

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
    [SerializeField, Header("Aura")]
    private Light2D         auraLight;
    [SerializeField, Header("Sound")]
    private AudioClip       torpedoLaunchSnd;
    [SerializeField]
    private AudioClip       hitSnd;
    [SerializeField]
    private AudioClip       startGrabSnd;
    [SerializeField]
    private AudioClip       endGrabSnd;
    [SerializeField]
    private AudioSource     engineAudioSrc;
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
    private Vector2         prevVelocity;

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

    private void HealthSystem_onHit(HealthSystem.DamageType damageType, float damage, Vector3 damagePosition, Vector3 damageNormal)
    {
        if (damageNormal.magnitude > 0)
        {
            rb.linearVelocity = Vector2.Reflect(prevVelocity, damageNormal);

            transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.linearVelocity.Perpendicular());

            if (hitSnd)
            {
                SoundManager.PlaySound(SoundType.PrimaryFX, hitSnd, 1.0f, UnityEngine.Random.Range(0.9f, 1.1f));
            }
        }
        if ((hitFlash == null) || (hitFlash.isFinished))
        {
            hitFlash = spriteEffect.FlashColor(0.1f, Color.red);
        }
        if (damageType != HealthSystem.DamageType.OverTime)
        {
            noControlTime = healthSystem.invulnerabilityTime * 0.5f;
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

        float ms = maxSpeed;
        if (inventoryType) ms *= inventoryType.speedModifier;
        ms *= GetAura(transform.position, playerId);
        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, 0, ms);

        rb.linearVelocity = prevVelocity = velocity;
    }

    private void Update()
    {
        if (engineAudioSrc)
        {
            float normalizedSpeed = Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed);
            engineAudioSrc.volume = 0.5f * Mathf.Clamp01(normalizedSpeed * 10.0f);
            engineAudioSrc.pitch = 0.5f + 0.5f * normalizedSpeed;
        }

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

        if (LevelManager.weaponsFree)
        {
            if (shootControl.IsDown())
            {
                Shoot();
            }
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
                    if (endGrabSnd) SoundManager.PlaySound(SoundType.PrimaryFX, endGrabSnd, 1.0f, 1.0f);

                    inventoryType = grabResource.data;
                    inventoryQuantity++;

                    Destroy(grabResource.gameObject);
                    grabResource = null;

                    if (inventoryQuantity == 1)
                    {
                        if (!string.IsNullOrEmpty(inventoryType.displayCombatText))
                        {
                            CombatTextManager.SpawnText(gameObject,
                                                        inventoryType.displayCombatText,
                                                        inventoryType.displayColor,
                                                        inventoryType.displayColor.ChangeAlpha(0.0f),
                                                        1.0f,
                                                        1.0f);
                        }
                    }

                }
            }
        }
        else
        {
            rope.enabled = false;
        }

        if (inventoryType != null)
        {
            float hr = inventoryQuantity * inventoryType.healthRegen * Time.deltaTime;
            if (hr > 0.0f)
            {
                healthSystem.Heal(hr, false);
            }
            else if (hr < 0.0f)
            {
                healthSystem.DealDamage(HealthSystem.DamageType.OverTime, Mathf.Abs(hr), transform.position, Vector3.zero);
            }

            if (inventoryType.speedAura != 1.0f)
            {
                auraLight.enabled = true;
                auraLight.color = inventoryType.speedAuraColor;
                auraLight.pointLightInnerRadius = inventoryType.speedAuraRadius * 0.5f;
                auraLight.pointLightOuterRadius = inventoryType.speedAuraRadius;
            }
            else
            {
                auraLight.enabled = false;
            }
        }
        else
        {
            auraLight.enabled = false;
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

        healthSystem.DealDamage(HealthSystem.DamageType.Burst, damage, collision.contacts[0].point, collision.contacts[0].normal);

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
        torpedo.speedModifier = (inventoryType) ? (inventoryType.weaponSpeedModifier) : (1.0f);
        var rb = torpedo.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = this.rb.linearVelocity;
        }
        if (torpedoLaunchSnd) SoundManager.PlaySound(SoundType.PrimaryFX, torpedoLaunchSnd, 1.0f, UnityEngine.Random.Range(0.75f, 1.0f));
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

                    if (startGrabSnd) SoundManager.PlaySound(SoundType.PrimaryFX, startGrabSnd, 1.0f, 1.0f);
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

    public static float GetAura(Vector3 position, int playerId)
    {
        float speed = 1.0f;

        var players = FindObjectsByType<Submarine>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.playerId == playerId) continue;

            if (player.inventoryType)
            {
                // Check distance
                float distance = Vector3.Distance(position, player.transform.position);

                if (distance < player.inventoryType.speedAuraRadius)
                {
                    speed *= player.inventoryType.speedAura;
                }
            }
        }

        return speed;
    }

    internal InputControl GetAttackControl()
    {
        return shootControl;
    }

    internal InputControl GetGatherControl()
    {
        return gatherControl;
    }
}
