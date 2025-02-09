using UnityEngine;

public class OxygenDisplay : ResourceBar
{
    City city;

    void Awake()
    {
        city = GetComponentInParent<City>();
    }

    protected override float GetNormalizedResource()
    {
        return city.oxygenPercentage;
    }

    protected override float GetResourceCount()
    {
        return city.oxygenCount;
    }
}
