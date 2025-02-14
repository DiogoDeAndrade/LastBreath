using NaughtyAttributes;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Bubble/Resource Data")]
public class ResourceData : ScriptableObject
{
    public string displayName;
    public string displayCombatText;
    public Color  displayColor = Color.white;
    public Sprite nodeImage;
    public Sprite resourceImage;
    public float  valueMultiplier = 1.0f;
    public float healthRegen = 0.0f;
    public float speedModifier = 1.0f;
    public float weaponSpeedModifier = 1.0f;
    public float speedAura = 1.0f;
    [ShowIf(nameof(hasSpeedAura))]
    public float speedAuraRadius = 200.0f;
    [ShowIf(nameof(hasSpeedAura))]
    public Color speedAuraColor = Color.white;
    bool hasSpeedAura => speedAura != 1.0f;
}

#if UNITY_6000_0_OR_NEWER

[Serializable]
public class ResourceDataProbList : ProbList<ResourceData> { }

#endif
