using UnityEngine;
using UC;

public class DamageRadius : MonoBehaviour
{
    [SerializeField] HealthSystem.DamageType    damageType = HealthSystem.DamageType.OverTime;
    [SerializeField] private float              radius = 75;
    [SerializeField] private bool               hemisphere;
    [SerializeField] private float              damage = 10.0f;
    [SerializeField] private float              damageOverDistance = 0.0f;
    [SerializeField] private LayerMask          layers;
    [SerializeField] private AudioSource        damageSound;

    private void Start()
    {
        if (damageType == HealthSystem.DamageType.Burst)
        {
            RunDamage();

            Destroy(this);
        }
    }

    void Update()
    {
        RunDamage();
    }

    void RunDamage()
    { 
        float volume = 0.0f;

        Collider2D[]    allObjectsInBox = null;
        var allObjectsInRadius = Physics2D.OverlapCircleAll(transform.position, radius, layers);
        if ((hemisphere) && (allObjectsInRadius.Length > 0))
        {
            allObjectsInBox = Physics2D.OverlapBoxAll(transform.position + transform.up * radius * 0.5f, new Vector2(radius * 2.0f, radius), transform.rotation.eulerAngles.z, layers);
        }
        foreach (var obj in allObjectsInRadius)
        {
            if (allObjectsInBox != null)
            {
                // Find this object in the box
                bool found = false;
                foreach (var c in allObjectsInBox)
                {
                    if (c == obj)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) continue;
            }
            var hs = obj.GetComponent<HealthSystem>();
            if (hs != null)
            {
                float distance = Vector3.Distance(hs.transform.position, transform.position);

                float actualDamage = Mathf.Max(0, damage + damageOverDistance * distance);

                if (damageType == HealthSystem.DamageType.OverTime) actualDamage *= Time.deltaTime;

                hs.DealDamage(damageType, actualDamage, transform.position, Vector3.zero, gameObject);
                volume = 1.0f;
            }
        }

        if (damageSound) damageSound.volume = volume;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (hemisphere)
            DebugHelpers.DrawHemisphere(transform.position, transform.right, transform.up, radius);
        else
            Gizmos.DrawWireSphere(transform.position, radius);
    }
}
