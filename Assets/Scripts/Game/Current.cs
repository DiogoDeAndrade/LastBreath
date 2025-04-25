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

    [SerializeField] 
    private CurrentLineFX   lineFXPrefab;

    private PathXY              path;
    private List<CurrentLineFX> lines = new();

    static private List<Current> Currents;

    void Start()
    {
        path = GetComponent<PathXY>();
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
        path = GetComponent<PathXY>();
        if (path == null) return;

        Gizmos.color = Color.green;

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

    static public Vector2 GetCurrentsStrength(Vector3 pos)
    {
        Vector3 force = Vector3.zero;

        if (Currents == null) return force;

        foreach (var current in Currents)
        {
            (var distance, var closestPoint, var direction) = current.path.GetDistance(pos);
            if (distance < current.range)
            {
                float strengthScale = 1.0f - (distance / current.range);
                force = direction * current.strength * Mathf.Pow(strengthScale, current.decayPower);
            }
        }

        return force;
    }
}
