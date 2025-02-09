using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public const int MaxWeapon = 2;

    [SerializeField] private int            _slot = 0;    // 1 = primary (trigger shot), 2 = secondary (square)
    [SerializeField] private int            _maxAmmo;
    [SerializeField] private float          cooldown;
    [SerializeField] private Projectile     projectilePrefab;
    [SerializeField] private bool           inheritVelocity;
    [SerializeField] private WeaponDisplay  uiPrefab;
    [SerializeField] private AudioClip      shootSnd;


    WeaponDisplay   weaponDisplay;
    int             _ammo;
    float           cooldownTimer;
    int             playerId;
    Rigidbody2D     submarineRB;

    public void Init(Submarine submarine, RectTransform UIParentObject)
    {
        if (weaponDisplay != null) return;

        weaponDisplay = Instantiate(uiPrefab, UIParentObject);
        weaponDisplay.weapon = this;

        playerId = submarine.playerId;
        submarineRB = submarine.GetComponent<Rigidbody2D>();
    }

    public bool canShoot => (cooldown <= 0) && ((_ammo > 0) || (maxAmmo == 0));
    public int ammo => _ammo;
    public float normalizedAmmo => (maxAmmo != 0) ? (_ammo / (float)maxAmmo) : (1.0f);
    public int maxAmmo => _maxAmmo;
    public int slot => _slot;

    private void Start()
    {
        _ammo = _maxAmmo;
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public void Shoot(float weaponSpeedModifier)
    {
        if (_ammo <= 0) return;
        if (cooldownTimer > 0) return;

        _ammo--;
        cooldownTimer = cooldown;

        var torpedo = Instantiate(projectilePrefab, transform.position, transform.rotation);
        torpedo.SetPlayerId(playerId);
        torpedo.speedModifier = weaponSpeedModifier;
        var rb = torpedo.GetComponent<Rigidbody2D>();
        if ((rb) && (inheritVelocity) && (submarineRB))
        {
            rb.linearVelocity = submarineRB.linearVelocity;
        }
        if (shootSnd) SoundManager.PlaySound(SoundType.PrimaryFX, shootSnd, 1.0f, UnityEngine.Random.Range(0.75f, 1.0f));
    }

    public void AddAmmo(int delta)
    {
        _ammo += delta;
        if (_ammo > maxAmmo) _ammo = _maxAmmo;
    }
}
