using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UC;
using UnityEngine.InputSystem;

public class Submarine : MonoBehaviour
{
    [SerializeField] 
    private int             _playerId = 0;
    [SerializeField] 
    private SubData         _subData;   
    [SerializeField]
    private float           maxSpeed = 200.0f; 
    [SerializeField]
    private float           acceleration = 200.0f;
    [SerializeField] 
    private float           maxRotationSpeed = 720.0f;
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
    [SerializeField, Header("UI")]
    private RectTransform   uiWeaponContainer;
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
    [SerializeField, Header("Lights")]
    private Light2D         auraLight;
    [SerializeField]
    private Light2D         subLight;
    [SerializeField, Header("FX")]
    private LineRenderer    lightningPrefab;
    [SerializeField, Header("Sound")]
    private AudioClip       hitSnd;
    [SerializeField]
    private AudioClip       startGrabSnd;
    [SerializeField]
    private AudioClip       endGrabSnd;
    [SerializeField]
    private AudioSource     engineAudioSrc;
    [SerializeField]
    private float           engineAudioMaxVolume = 0.5f;
    [SerializeField]
    private AudioSource     electroStunSrc;
    [SerializeField, Header("Input")] 
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))] 
    private UC.InputControl     moveControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl[]   fireControl = new UC.InputControl[2];
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl    gatherControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl    dropControl;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl    pauseControl;

    private Vector2             movementVector;
    private Rigidbody2D         rb;
    private Collider2D          mainCollider;
    private HealthSystem        healthSystem;
    private SpriteEffect        spriteEffect;
    private CameraFollowTarget  cameraFollowTarget;
    private float               cooldownTimer;
    private float               noControlTime;
    private Resource            grabResource;
    private float               grabT;
    private bool                grabGoingOut;
    private Vector3             grabOriginalPosition;
    private ResourceData        inventoryType;
    private int                 inventoryQuantity;
    private Vector2             prevVelocity;
    private Weapon[]            weapons;
    private bool                lightningEffect;
    private float               lightingEffectTimer;

    private Tweener.BaseInterpolator healthGainEffect;
    private Tweener.BaseInterpolator hitFlash;

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
        _subData = pd.submarine;

        // Setup sub
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer)
        {
            spriteRenderer.sprite = _subData.primarySprite;
        }
        for (int i = 0; i < Mathf.Min(debries.Length, _subData.debrieSprites.Length); i++)
        {
            if (debries[i] == null) continue;

            spriteRenderer = debries[i].GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                spriteRenderer.sprite = _subData.debrieSprites[i];
            }
        }
        if (subLight)
        {
            subLight.lightCookieSprite = _subData.lightSprite;
        }

        cameraFollowTarget = GetComponent<CameraFollowTarget>();

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
            if (subLight)
            {
                subLight.color = playerCustomization.GetLightColor();
            }
        }

        MasterInputManager.SetupInput(_playerId, playerInput);
        moveControl.playerInput = playerInput;
        foreach (var f in fireControl) f.playerInput = playerInput;
        gatherControl.playerInput = playerInput;
        dropControl.playerInput = playerInput;
        pauseControl.playerInput = playerInput;

        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.maxHealth = _subData.health;
        healthSystem.ResetHealth();
        healthSystem.onHit += HealthSystem_onHit;
        healthSystem.onHeal += HealthSystem_onHeal;
        healthSystem.onDead += HealthSystem_onDead;

        if (subLight)
        {
            var playerLightIntensity = LevelManager.playerLightIntensity;
            subLight.enabled = playerLightIntensity > 0.0f;
            subLight.intensity = playerLightIntensity;
        }

        if (LevelManager.overrideWeapons)
        {
            var currentWeapons = GetComponentsInChildren<Weapon>();
            foreach (var w in currentWeapons)
            {
                Destroy(w.gameObject);
            }
            var newWeapons = LevelManager.overrideWeaponPrefabs;
            if (newWeapons != null)
            {
                foreach (var w in newWeapons)
                {
                    Instantiate(w, transform);
                }
            }
        }

        weapons = new Weapon[Weapon.MaxWeapon];
        var childWeapons = GetComponentsInChildren<Weapon>();
        foreach (var w in childWeapons)
        {
            weapons[w.slot - 1] = w;
            w.Init(this, uiWeaponContainer);
            w.transform.localPosition = _subData.weaponOffset;
            if ((_subData.ammoMultiplier != 1.0f) && (w.maxAmmo > 0))
            {
                w.maxAmmo = Mathf.CeilToInt(w.maxAmmo * _subData.ammoMultiplier);
            }
        }

        maxSpeed = _subData.speed;
        acceleration = _subData.acceleration;
        maxRotationSpeed = _subData.rotationSpeed;
        drag = _subData.drag;

        switch (_subData.colliderType)
        {
            case SubData.ColliderType.Capsule:
                CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
                capsuleCollider.enabled = true;
                capsuleCollider.size = _subData.size;
                capsuleCollider.offset = _subData.offset;
                mainCollider = capsuleCollider;
                break;
            default:
                break;
        }
    }

    private void HealthSystem_onHeal(float healthGain, GameObject sourceHeal)
    {
        if ((healthGainEffect == null) || (healthGainEffect.isFinished))
        {
            healthGainEffect = spriteEffect.FlashColor(0.5f, Color.green);
        }
    }

    private void HealthSystem_onDead(GameObject sourceDamage)
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

        // Notify city
        var cities = FindObjectsByType<City>(FindObjectsSortMode.None);
        foreach (var c in cities)
        {
            if (c.playerId == playerId)
                c.NotifyPlayerDeath(this);
        }

        Destroy(gameObject);
    }

    private void HealthSystem_onHit(HealthSystem.DamageType damageType, float damage, Vector3 damagePosition, Vector3 damageNormal, GameObject sourceDamage)
    {
        if (damageNormal.magnitude > 0)
        {
            rb.linearVelocity = Vector2.Reflect(prevVelocity, damageNormal);

            transform.rotation = Quaternion.LookRotation(Vector3.forward, rb.linearVelocity.Perpendicular());

            if (hitSnd)
            {
                SoundManager.PlaySound(SoundType.PrimaryFX, hitSnd, 1.0f, UnityEngine.Random.Range(0.9f, 1.1f));
            }

            CameraShake2d.Shake(2, 0.05f);
        }
        if ((hitFlash == null) || (hitFlash.isFinished))
        {
            hitFlash = spriteEffect.FlashColor(0.1f, Color.red);
        }
        if (damageType != HealthSystem.DamageType.OverTime)
        {
            noControlTime = healthSystem.invulnerabilityTime * 0.5f;
        }

        cameraFollowTarget?.FreezeFollow(0.5f);
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

        var currentForce = Current.GetCurrentsStrength(transform.position);
        velocity = velocity + currentForce * Time.fixedDeltaTime;

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
            engineAudioSrc.volume = engineAudioMaxVolume * Mathf.Clamp01(normalizedSpeed * 10.0f);
            engineAudioSrc.pitch = 0.5f + 0.5f * normalizedSpeed;
        }

        movementVector = moveControl.GetAxis2();
        if (noControlTime > 0) movementVector = Vector2.zero;

        if (!healthSystem.isInvulnerable)
        {
            var velocity = movementVector;
            if (movementVector.sqrMagnitude > 1e-3)
            {
                var direction = movementVector.normalized;

                var targetRotation = Quaternion.LookRotation(Vector3.forward, new Vector2(-velocity.y, velocity.x));

                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * maxRotationSpeed);
            }
        }

        if (LevelManager.weaponsFree)
        {
            for (int i = 0; i < fireControl.Length; i++)
            { 
                if (weapons[i] == null) continue;
                if (((weapons[i].canHold) && (fireControl[i].IsPressed())) ||
                    ((!weapons[i].canHold) && (fireControl[i].IsDown())))
                {
                    float damageModifier = (inventoryType) ? (inventoryType.weaponDamageModifier) : (1.0f);
                    damageModifier *= _subData.damageMultiplier;
                    float speedModifier = (inventoryType) ? (inventoryType.weaponSpeedModifier) : (1.0f);
                    weapons[i]?.Shoot(damageModifier, speedModifier);
                }
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
                healthSystem.Heal(hr, false, gameObject);
            }
            else if (hr < 0.0f)
            {
                healthSystem.DealDamage(HealthSystem.DamageType.OverTime, Mathf.Abs(hr), transform.position, Vector3.zero, gameObject);
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

        if (lightningEffect)
        {
            if (noControlTime <= 0.0f)
            {
                lightningEffect = false;
                electroStunSrc.FadeTo(0.0f, 0.1f);
            }
            else
            {
                lightingEffectTimer -= Time.deltaTime;
                if (lightingEffectTimer <= 0.0f)
                {
                    DoBolt();
                    lightingEffectTimer = 0.1f;
                }
            }
        }

        if (pauseControl.IsDown())
        {
            var pauseMenu = FindFirstObjectByType<PauseMenu>();
            if (pauseMenu != null)
            {
                if (!pauseMenu.isPaused)
                {
                    pauseMenu.Pause(playerInput);
                }
                else
                {
                    pauseMenu.Unpause(playerInput);
                }
            }
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

        healthSystem.DealDamage(HealthSystem.DamageType.Burst, damage, collision.contacts[0].point, collision.contacts[0].normal, collision.gameObject);

        // Check if collision was with another thing with health
    }

    public Weapon GetWeapon(int i)
    {
        return weapons[i];
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

    internal UC.InputControl GetAttackControl()
    {
        return fireControl[1];
    }

    internal UC.InputControl GetGatherControl()
    {
        return gatherControl;
    }

    public void FreezeControl(float time)
    {
        noControlTime = time;
    }

    public void EnableLightningEffect()
    {
        lightningEffect = true;
        lightingEffectTimer = 0.0f;
        electroStunSrc.FadeTo(1.0f, 0.1f);
    }

    void DoBolt()
    {
        var lineRenderer = Instantiate(lightningPrefab);
        lineRenderer.FadeOut(0.25f).Done(() => Destroy(lineRenderer.gameObject));

        Bounds bounds = mainCollider.bounds;
        
        List<Vector3> positions = new();
        int nHops = Random.Range(4, 8);
        
        for (int i = 0; i < nHops; i++)
        {
            positions.Add(bounds.Random());
        }

        if (lineRenderer.numCapVertices > 0)
        {
            Vector3 delta1 = (positions[1] - positions[0]);
            if (delta1.sqrMagnitude > 0) delta1 = delta1.normalized * lineRenderer.widthCurve.Evaluate(0) * lineRenderer.widthMultiplier * 0.5f;
            Vector3 delta2 = (positions[positions.Count - 1] - positions[positions.Count - 2]);
            if (delta2.sqrMagnitude > 0) delta2 = delta1.normalized * lineRenderer.widthCurve.Evaluate(0) * lineRenderer.widthMultiplier * 0.5f;

            positions[0] = positions[0] + delta1;
            positions[positions.Count - 1] = positions[positions.Count - 1] + delta2;
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());

        lineRenderer.enabled = true;
    }
}
