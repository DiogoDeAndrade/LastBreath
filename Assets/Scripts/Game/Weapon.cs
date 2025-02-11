using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Weapon : MonoBehaviour
{
    public const int MaxWeapon = 2;

    [SerializeField] 
    private int            _slot = 0;    // 1 = primary (trigger shot), 2 = secondary (square)
    [SerializeField] 
    private bool           _canHold;
    [SerializeField] 
    private int            _maxAmmo;
    [SerializeField] 
    private float          cooldown;
    [SerializeField] 
    private Projectile     projectilePrefab;
    [SerializeField] 
    private bool           usePositionNoise;
    [SerializeField, ShowIf(nameof(usePositionNoise))] 
    private float          noiseRadius = 5.0f;
    [SerializeField]
    private bool           useRotationNoise; 
    [SerializeField, ShowIf(nameof(useRotationNoise))] 
    private float          noiseAngle = 15.0f;
    [SerializeField] 
    private bool           inheritVelocity;
    [SerializeField] 
    private WeaponDisplay  uiPrefab;
    [SerializeField] 
    private AudioClip      shootSnd;
    [SerializeField] 
    private AudioSource    shootAudioSource;
    [SerializeField]
    private Light2D        shootLight;


    WeaponDisplay   weaponDisplay;
    int             _ammo;
    float           cooldownTimer;
    int             playerId;
    Rigidbody2D     submarineRB;
    float           shootIntensity;
    float           shootAudioSourceVolume;

    public void Init(Submarine submarine, RectTransform UIParentObject)
    {
        if (weaponDisplay != null) return;

        if (uiPrefab)
        {
            weaponDisplay = Instantiate(uiPrefab, UIParentObject);
            weaponDisplay.weapon = this;
        }

        playerId = submarine.playerId;
        submarineRB = submarine.GetComponent<Rigidbody2D>();
        if (shootLight)
        {
            shootIntensity = shootLight.intensity;
            shootLight.intensity = 0;
        }
    }

    public bool canShoot => (cooldown <= 0) && ((_ammo > 0) || (maxAmmo == 0));
    public bool canHold => _canHold;
    public int ammo => _ammo;
    public float normalizedAmmo => (maxAmmo != 0) ? (_ammo / (float)maxAmmo) : (1.0f);
    public int maxAmmo => _maxAmmo;
    public int slot => _slot;

    private void Start()
    {
        _ammo = _maxAmmo;

        if (shootAudioSource)
        {
            shootAudioSourceVolume = shootAudioSource.volume;
            shootAudioSource.volume = 0.0f;
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else
        {
            if (shootAudioSource)
            {
                shootAudioSource.volume = Mathf.Clamp01(shootAudioSource.volume - Time.deltaTime / cooldown);
            }
        }

        if (shootLight)
        {
            shootLight.intensity = Mathf.Lerp(0.0f, shootIntensity, Mathf.Clamp01(cooldownTimer / cooldown));
        }
    }

    public void Shoot(float weaponSpeedModifier)
    {
        if ((_ammo <= 0) && (_maxAmmo > 0)) return;
        if (cooldownTimer > 0) return;

        if (_maxAmmo > 0) _ammo--;
        cooldownTimer = cooldown;

        Vector3     position = transform.position;
        Quaternion  rotation = transform.rotation;
        if (usePositionNoise)
        {
            position = position.xy() + UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0.0f, noiseRadius);
            position.z = transform.position.z;
        }
        if (useRotationNoise)
        {
            rotation = rotation * Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(-noiseAngle, noiseAngle));
        }

        var torpedo = Instantiate(projectilePrefab, position, rotation);
        torpedo.SetPlayerId(playerId);
        torpedo.speedModifier = weaponSpeedModifier;
        var rb = torpedo.GetComponent<Rigidbody2D>();
        if ((rb) && (inheritVelocity) && (submarineRB))
        {
            rb.linearVelocity = submarineRB.linearVelocity;
        }
        if (shootSnd) SoundManager.PlaySound(SoundType.PrimaryFX, shootSnd, 1.0f, UnityEngine.Random.Range(0.75f, 1.0f));
        if (shootAudioSource) shootAudioSource.volume = shootAudioSourceVolume;
        if (shootLight) shootLight.intensity = shootIntensity;
    }

    public void AddAmmo(int delta)
    {
        _ammo += delta;
        if (_ammo > maxAmmo) _ammo = _maxAmmo;
    }
}
