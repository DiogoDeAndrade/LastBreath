using UnityEngine;

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
        if (LevelManager.weaponsFree) return weapon.ammo;

        return 0;
    }
}
