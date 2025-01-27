using UnityEngine;

public class AmmoDisplay : ResourceBar
{
    [Header("Ammo Source")]
    public Submarine submarine;

    protected override float GetNormalizedResource()
    {
        return submarine.normalizedAmmo;
    }

    protected override float GetResourceCount()
    {
        if (LevelManager.weaponsFree) return submarine.ammo;

        return 0;
    }
}
