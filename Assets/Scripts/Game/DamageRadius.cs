using UnityEngine;
using static HealthSystem;

public class DamageRadius : MonoBehaviour
{
    [SerializeField] private float          radius = 75;
    [SerializeField] private bool           hemisphere;
    [SerializeField] private float          damage = 10.0f;
    [SerializeField] private LayerMask      layers;
    [SerializeField] private AudioSource    damageSound;


    void Update()
    {
        float volume = 0.0f;

        Collider2D[]    allObjectsInBox = null;
        var allObjectsInRadius = Physics2D.OverlapCircleAll(transform.position, radius, layers);
        if (hemisphere)
        {
            allObjectsInBox = Physics2D.OverlapBoxAll(transform.position + Vector3.up * radius * 0.5f, new Vector2(radius * 2.0f, radius), 0.0f, layers);
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
                hs.DealDamage(DamageType.OverTime, damage * Time.deltaTime, transform.position, Vector3.zero, gameObject);
                volume = 1.0f;
            }
        }

        if (damageSound) damageSound.volume = volume;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
