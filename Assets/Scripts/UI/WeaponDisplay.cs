using UnityEngine;
using UC;

public class WeaponDisplay : ResourceBar
{
    [Header("Ammo Source")]
    public Weapon weapon;

    protected override float GetNormalizedResource()
    {
        return weapon.normalizedAmmo;
    }

    protected override float GetResourceCount()
    {
        if (LevelManager.weaponsFree)
        {
            return weapon.ammo;
        }

        return 0;
    }

    protected override void Update()
    {
        base.Update();

        if (weapon == null)
        {
            Destroy(gameObject);
        }
    }
}
