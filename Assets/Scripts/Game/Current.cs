using NaughtyAttributes;
using UC;
using UnityEngine;
using System.Collections.Generic;

public class Current : MonoBehaviour
{
    [SerializeField]
    private int             lineCount = 5;
    [SerializeField, MinMaxSlider(0.5f, 10.0f)]
    private Vector2         startTime;
    [SerializeField]
    private float           range = 20.0f;
    [SerializeField]
    private float           strength = 100.0f;
    [SerializeField]
    private float           decayPower = 2.0f;
    [SerializeField, ShowIf(nameof(isBoxCurrent))]
    private Vector2         currentDirection = Vector2.up;

    [SerializeField] 
    private CurrentLineFX   lineFXPrefab;

    private PathXY              path;
    private BoxCollider2D       boxCollider;
    private List<CurrentLineFX> lines = new();

    bool isBoxCurrent => (GetComponent<PathXY>() == null) && (GetComponent<BoxCollider2D>() != null);

    public Vector2 direction => currentDirection;

    static private List<Current> Currents;

    void Start()
    {
        path = GetComponent<PathXY>();
        boxCollider = GetComponent<BoxCollider2D>();
        for (int i = 0; i < lineCount; i++)
        {
            Invoke(nameof(NewCurrentTrail), startTime.Random());
        }

        if (Currents == null) Currents = new();
        Currents.Add(this);
    }

    private void OnDestroy()
    {
        if (Currents == null) return;
        Currents.Remove(this);
    }

    void NewCurrentTrail()
    {
        var newTrail = Instantiate(lineFXPrefab, transform);
        newTrail.name = "CurrentTrailFX";

        InitTrail(newTrail);

        lines.Add(newTrail);
    }

    void InitTrail(CurrentLineFX trail)
    {
        float startT = Random.Range(0.0f, 1.0f);
        float endT= Random.Range(0.0f, 1.0f);
        trail.Init(Mathf.Min(startT, endT), Mathf.Max(startT, endT), Random.insideUnitCircle.xy0() * range);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var line in lines)
        {
            if (!line.isRunning)
            {
                InitTrail(line);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        path = GetComponent<PathXY>();
        if (path != null)
        {
            Vector3 pa, pb;
            pa = pb = Vector3.zero;

            var points = path.GetPoints();
            for (int i = 0; i < points.Count - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                var dir = (b - a).normalized.PerpendicularXY();

                var da = a + dir * range;
                var db = a - dir * range;

                if (i > 0)
                {
                    Gizmos.DrawLine(da, pa);
                    Gizmos.DrawLine(db, pb);
                }

                pa = da;
                pb = db;
            }
        }
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            DebugHelpers.DrawArrow(transform.position, currentDirection, range, range * 0.2f, currentDirection.Perpendicular());
        }
    }

    Vector3 GetForce(Vector3 pos)
    {
        Vector3 force = Vector3.zero;

        if (path != null)
        {
            (var distance, var closestPoint, var direction) = path.GetDistance(pos);
            if (distance < range)
            {
                float strengthScale = 1.0f - (distance / range);
                force = direction * strength * Mathf.Pow(strengthScale, decayPower);
            }
        }
        else if (boxCollider != null)
        {
            Vector3 closestPoint = boxCollider.ClosestPoint(pos);

            bool isInside = boxCollider.bounds.Contains(pos) && (closestPoint == pos);

            float strengthScale = 0.0f;

            if (isInside)
            {
                strengthScale = 1.0f;
            }
            else
            {
                float distance = Vector3.Distance(pos, closestPoint);
                if (distance < range)
                {
                    strengthScale = 1.0f - (distance / range);
                }
            }

            if (strengthScale > 0.0f)
            {
                // Define your direction here. Example: rightwards in local space
                force = currentDirection * strength * Mathf.Pow(strengthScale, decayPower);
            }
        }

        return force;
    }

    static public Vector2 GetCurrentsStrength(Vector3 pos)
    {
        Vector3 force = Vector3.zero;

        if (Currents == null) return force;

        foreach (var current in Currents)
        {
            force += current.GetForce(pos);
        }

        return force;
    }
}
