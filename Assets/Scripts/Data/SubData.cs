using System;
using UnityEngine;
using UC;

[CreateAssetMenu(fileName = "SubData", menuName = "Bubble/Submarine Data")]
public class SubData : UIDiscreteSOOption, IEquatable<SubData>
{
    public enum ColliderType { Capsule };

    [Header("Visuals")]
    public string   displayName;
    public Sprite   primarySprite;
    public Sprite[] debrieSprites;
    public Sprite   lightSprite;
    [Header("Weapons")]
    public Vector2  weaponOffset = new Vector2(22.0f, 0.0f);
    public float    ammoMultiplier = 1.0f;
    public float    damageMultiplier = 1.0f;
    [Header("Collider")]
    public ColliderType colliderType = ColliderType.Capsule;
    public Vector2      offset;
    public Vector2      size;
    [Header("Stats")]
    public float    health = 100.0f;
    public float    speed = 200.0f;
    public float    acceleration = 200.0f;
    public float    rotationSpeed = 720.0f;
    public float    drag = 1.0f;
    [Header("Display Stats")]
    public int      starsHull = 1;
    public int      starsDamage = 1;
    public int      starsSpeed = 1;
    public int      starsManoeuvrability = 1;

    public override string GetDisplayName()
    {
        return displayName;
    }

    public bool Equals(SubData other)
    {
        return this == other;
    }
}
