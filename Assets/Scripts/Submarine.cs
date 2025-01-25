using NaughtyAttributes;
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
    [SerializeField]
    private float           collisionDamagePerSpeed = 0.1f;
    [SerializeField, Header("Input")] 
    private PlayerInput     playerInput;
    [SerializeField, InputPlayer(nameof(playerInput))] 
    private InputControl    moveControl;
    [SerializeField]
    private GameObject      explosionObject;
    [SerializeField]
    private GameObject[]    debries;

    private Vector2         movementVector;
    private Rigidbody2D     rb;
    private HealthSystem    healthSystem;
    private SpriteEffect    spriteEffect;

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

        rb = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.onHit += HealthSystem_onHit;
        healthSystem.onDead += HealthSystem_onDead;
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var damage = collision.relativeVelocity.magnitude * collisionDamagePerSpeed;
        healthSystem.DealDamage(damage, collision.contacts[0].point, collision.contacts[0].normal);

        // Check if collision was with another thing with health
    }

    [Button("Unpair all")]
    void UnpairAll()
    {
        playerInput.user.UnpairDevices();
    }

    [Button("Pair device")]
    void PairDevice()
    {
        MasterInputManager.SetupInput(_playerId, playerInput);
    }

}
