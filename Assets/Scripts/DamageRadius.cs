using UnityEngine;
using static HealthSystem;

public class DamageRadius : MonoBehaviour
{
    [SerializeField] private float      radius = 75;
    [SerializeField] private float      damage = 10.0f;
    [SerializeField] private LayerMask  layers;

    void Update()
    {
        var allObjectsInRadius = Physics2D.OverlapCircleAll(transform.position, radius, layers);
        foreach (var obj in allObjectsInRadius)
        {
            var hs = obj.GetComponent<HealthSystem>();
            if (hs != null)
            {
                hs.DealDamage(DamageType.OverTime, damage * Time.deltaTime, transform.position, Vector3.zero);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
