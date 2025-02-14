using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;

public class RotateToPOI : MonoBehaviour
{
    [SerializeField] private Hypertag[] tags;
    [SerializeField] private float      minRange = 0.0f;
    [SerializeField] private float      maxRange = 200.0f;
    [SerializeField] private float      angularTolerance = 45.0f;
    [SerializeField] private float      maxRotationSpeed = 360.0f;
    [SerializeField] private bool       checkHealth;
    [SerializeField] private bool       checkLOS;
    [SerializeField, ShowIf(nameof(checkLOS))] private float distanceBias;
    [SerializeField, ShowIf(nameof(checkLOS))] private LayerMask occluderLayers;

    Quaternion initialLocalRotation;

    void Start()
    {
        initialLocalRotation = transform.localRotation;
    }

    void Update()
    {
        var objects = Hypertag.FindObjectsWithHypertag<Transform>(tags);
        var sortedObjects = objects.OrderBy(t => Vector3.Distance(t.position, transform.position));
        foreach (var obj in sortedObjects)
        {
            float d = Vector3.Distance(obj.position, transform.position);
            if (d < minRange) continue;
            if (d > maxRange) break;

            if (checkHealth)
            {
                var hs = obj.GetComponent<HealthSystem>();
                if (hs)
                {
                    if (hs.isDead) continue;
                }
            }
            if (checkLOS)
            {
                var dir = obj.position - transform.position;
                var dist = dir.magnitude + distanceBias;
                dir.SafeNormalize();
                var hit = Physics2D.Raycast(transform.position, dir, dist, occluderLayers);
                if (hit.collider != null) continue;
            }

            if (Track(obj.position))
            {
                return;
            }
        }

        // Reset to base position
        Track(transform.position + GetBaseRight() * maxRange);
    }

    bool Track(Vector3 targetPos)
    {        
        var baseRight = GetBaseRight();
        var toTarget = (targetPos - transform.position).normalized;
        float angle = Vector2.Angle(baseRight, toTarget);
        if (angle < angularTolerance)
        {
            // Follow this
            var direction = Quaternion.LookRotation(Vector3.forward, toTarget.PerpendicularXY());
            transform.rotation = Quaternion.RotateTowards(transform.rotation, direction, maxRotationSpeed * Time.deltaTime);

            return true;
        }

        return false;
    }

    Vector3 GetBaseRight() => ((transform.parent) ? (transform.parent.rotation* initialLocalRotation) : (initialLocalRotation)) * Vector3.right;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = new Color(0.0f, 1.0f, 0.0f, 0.1f);
        Handles.DrawSolidArc(transform.position, Vector3.forward, transform.right.RotateZ(-angularTolerance), angularTolerance * 2.0f, minRange);
        Handles.DrawSolidArc(transform.position, Vector3.forward, transform.right.RotateZ(-angularTolerance), angularTolerance * 2.0f, maxRange);
    }
#endif
}
